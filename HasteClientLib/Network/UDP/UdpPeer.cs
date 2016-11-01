/*
* Copyright 2016 NHN Entertainment Corp.
*
* NHN Entertainment Corp. licenses this file to you under the Apache License,
* version 2.0 (the "License"); you may not use this file except in compliance
* with the License. You may obtain a copy of the License at:
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Haste.Data;
using Haste.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using Haste.ByteBuffer;
using Haste.Messages;
using Haste.Network.Queues;

namespace Haste.Network
{
    internal partial class UdpPeer : INetworkPeer, INetworkBroker
    {
        private readonly Cipher _cipher = new Cipher();

        public Cipher Cipher { get { return _cipher; } }

        internal const int MtuHeaderLength = (int)Lengths.MtuHeader;
        internal const short MaxCustomDataLen = 200;

        private readonly bool _allowStatistics = false;

        private readonly object _renderingLoopLock = new object();

        private Queue<Action> _renderingLoopDispatchQueue = new Queue<Action>();
        private Queue<Action> _toPassQueue = new Queue<Action>();

        private IListener _listener;
        private volatile int _peerId = -1;

        private bool _isAckThreadRunning = false;
        private Thread _sendAckBackgroundThread = null;
        private int _serverTimeOffset;

        private RoundTripTime _roundTripTime;
        private readonly NetStatistics _statistics = new NetStatistics();

        private uint _timestampOfLastReliableSend; // A last received time of a reliable command.

        private UdpSocket _udpSocket;

        private readonly UdpChannelList _channels;
        
        private byte[] _udpBuffer; // This buffer's size equals to MTU size.

        private int _udpBufferIndex;

        private ConnectingBroker _connectionBroker;

        private ConnectionConfig _connectionConfig;
        
        public UdpPeer(byte[] customData, Version sdkVersion, Version clientVersion, IListener listener, ConnectionConfig config)
        {
            if (customData != null && customData.Length > MaxCustomDataLen)
            {
                throw new ArgumentException(string.Format("customData is less than MAX_CUSTOM_DATA_LEN {0}", MaxCustomDataLen));
            }

            _listener = listener;

            CustomData = customData;

            ClientVersion = clientVersion;
            SdkVersion = sdkVersion;

            _connectionConfig = config;
            _allowStatistics = config.AllowStatistics;

            Log(LogLevel.Debug,
                "Configure : ChannelCount[{0}] MTU[{1}] PingInterval[{2}] DisconnectTimeout[{3}] MaxUnreliableCommands[{4}] IsCrcEnabled[{5}] SentCountAllowance[{6}]",
                ChannelCount, MTU, PingInterval, DisconnectTimeout, MaxUnreliableCommands, IsCrcEnabled, SentCountAllowance);

            _channels = new UdpChannelList(config, ChannelCount + 1);
        }

        public event Action<LogLevel, string> LogMessageRecevied;

        public short ChannelCount { get { return _connectionConfig.ChannelCount; } }

        public uint AvailableSpace { get; private set; }

        public short MTU { get { return _connectionConfig.MtuSize; } }

        public byte[] CustomData { get; private set; }

        public Version ClientVersion { get; private set; }

        public int MaxUnreliableCommands { get { return _connectionConfig.MaxUnreliableCommands; } }

        /// <summary>
        /// After sending a reliable command, judges disconnection if exceed (sent time + disconnect timeout).
        /// </summary>
        public int DisconnectTimeout { get { return _connectionConfig.DisconnectionTimeout; } }

        public IPEndPoint RemoteEndPoint { get; private set; }
        
        public int PeerID { get { return _peerId; } }

        public int QueuedSentCommandCount
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _channels.Count; i++)
                {
                    count += _channels[i].ReliableSendQueue.RemainingSentCommandCount;
                }

                return count;
            }
        }

        internal Version SdkVersion { get; private set; }

        internal int SentCountAllowance { get { return _connectionConfig.SentCountAllowance; } }

        public uint ServerTime { get { return (uint)_serverTimeOffset + EnvironmentTimer.GetTickCount(); } }

        public SocketState SocketState { get { return _udpSocket.State; } }

        public NetStatistics Statistics { get { return _statistics; } }

        internal int PingInterval { get { return _connectionConfig.PingInterval; } }

        public uint MeanOfRoundTripTime { get { return (uint)_roundTripTime.MeanOfRoundTripTime; } }

        internal uint MeanOfRoundTripTimeVariance { get { return (uint)_roundTripTime.MeanOfRoundTripTimeVariance; } }

        public bool IsCrcEnabled { get { return _connectionConfig.IsCrcEnabled; } }

        /// <summary>
        /// Connect to a remote server
        /// </summary>
        /// <param name="remoteEndPoint">Address information of a remote server</param>
        /// <returns></returns>
        public bool Connect(IPEndPoint remoteEndPoint)
        {
            if (_udpSocket != null && _udpSocket.State != SocketState.Disconnected)
            {
                _udpSocket.Dispose();
            }

            _udpSocket = new UdpSocket(_connectionConfig.MtuSize, DisconnectTimeout);
            if(_connectionConfig.BindPortRange != null)
                _udpSocket.SetBindPort(_connectionConfig.BindPortRange.StartPort, _connectionConfig.BindPortRange.EndPort);

            _connectionBroker = new ConnectingBroker(_udpSocket, this);
            _udpSocket.SetBroker(_connectionBroker, SocketState.Connecting);

            _peerId = 0;
            _roundTripTime = new RoundTripTime();

            ResetQueues();

            RemoteEndPoint = remoteEndPoint;

            AvailableSpace = (uint)(MTU - (uint)Lengths.FragmentHeader - (uint)Lengths.MtuHeader);

            _udpBuffer = new byte[MTU];

            _cipher.GenerateLocalKeys();

            if (_udpSocket.Prepare(RemoteEndPoint))
            {
                _isAckThreadRunning = true;

                if (_sendAckBackgroundThread != null)
                    _sendAckBackgroundThread.Abort();

#if PERF
                TryToConnectAsync();
#else
                _sendAckBackgroundThread = new Thread(SendAckBackground);
                _sendAckBackgroundThread.Name = "SendAckThread";
                _sendAckBackgroundThread.IsBackground = true;
                _sendAckBackgroundThread.Start();
#endif
                _udpSocket.Send(_connectionBroker.CreateConnectCommand(
                    ChannelCount,
                    MTU,
                    DataSerializer.Version,
                    DisconnectTimeout,
                    IsCrcEnabled,
                    _cipher.PublicKey,
                    EnvironmentTimer.GetTickCount()));
            }
            else
            {
                if (_listener != null)
                {
                    string message = string.Format("Fail to connect to [{0}]{1}:{2} - SocketState: {3}", remoteEndPoint.AddressFamily, remoteEndPoint.Address, remoteEndPoint.Port, _udpSocket.State);
                    _listener.OnStatusChanged(StatusCode.FailedToConnect, message);
                }

                return false;
            }

            return true;
        }

        internal void Init(int peerID, int port, BigInteger serverKey, long sendingTime, long responseTime)
        {
            _peerId = peerID;
            _cipher.EstablishKeyExchange(serverKey);
            RemoteEndPoint.Port = port;

            long currentTime = EnvironmentTimer.GetTickCount();
            uint rtt = (uint)(currentTime - sendingTime);
            uint currentServerTime = (uint)(responseTime + (rtt >> 1));

            Interlocked.Exchange(ref _serverTimeOffset, (int)(currentServerTime - currentTime));
            _roundTripTime.MeanOfRoundTripTime = (int) rtt;

            if (Logger.IsDebugEnabled)
            {
                Log(LogLevel.Debug, "Initial PeerID[{0}] RTT[{1}] Servertime[{2}]", _peerId,
                    _roundTripTime.MeanOfRoundTripTime, currentServerTime);
            }

            byte[] bytes = new InitialRequest(SdkVersion, ClientVersion, CustomData).GetBytes();
            EnqueueOutgoingCommand(new OutgoingCommand(CommandType.Reliable, 0, bytes));

            _udpSocket.SetBroker(this, SocketState.Connected);
        }

        /// <summary>
        /// Enqueue to a rendering queue, this method is thread-safe.
        /// </summary>
        /// <param name="action"></param>
        private void EnqueueRenderingQueue(Action action)
        {
            lock (_renderingLoopLock)
            {
                _renderingLoopDispatchQueue.Enqueue(action);
            }
        }

        public void Log(LogLevel level, string format, params object[] args)
        {
            if (Logger.Current > level)
                return;

            var builder = new StringBuilder((args.Length > 0) ? string.Format(format, args) : format);

            if (level == LogLevel.Error)
            {
                var frames = new StackTrace().GetFrames();

                if (frames != null)
                {
                    foreach (var frame in frames)
                    {
                        builder.Append(" : ");
                        builder.Append(frame.GetMethod().Name);
                    }
                }
            }

            string message = builder.ToString();

            EnqueueRenderingQueue(() =>
            {
                if (LogMessageRecevied != null)
                    LogMessageRecevied(level, message);
            });
        }

        public void Disconnect(DisconnectReason reason, bool sendingRequired)
        {
            if (_udpSocket.State == SocketState.Disconnecting || _udpSocket.State == SocketState.Disconnected)
                return;

            Log(LogLevel.Debug, "Disconnect Reson[{0}]", reason.ToString());

            if (sendingRequired)
            {
                _udpSocket.Disconnecting();

                var buffer = ByteBufferFactory.NewBuffer();
                buffer.WriteInt((int)reason);

                EnqueueOutgoingCommand(new OutgoingCommand(CommandType.Disconnect, (byte)(ChannelCount - 1), buffer.ToArray()));
                FlushSendQueues();
            }

            _udpSocket.Dispose();

            if (_listener != null)
            {
                switch (reason)
                {
                    case DisconnectReason.Timeout:
                    case DisconnectReason.ServerUserLimit:
                    case DisconnectReason.ServerLogic:
                    case DisconnectReason.QueueFull:
                    case DisconnectReason.InvalidConnection:
                    case DisconnectReason.InvalidDataFormat:
                    case DisconnectReason.ClientDisconnect:
                    case DisconnectReason.ApplicationStop:
                        _listener.OnStatusChanged(StatusCode.Disconnected, string.Format("Disconnected by {0}", reason));
                        break;
                    case DisconnectReason.ConnectionFailed:
                        _listener.OnStatusChanged(StatusCode.FailedToConnect, string.Format("Failed to connect to {0}:{1}", RemoteEndPoint.Address, RemoteEndPoint.Port));
                        break;
                    default:
                        _listener.OnStatusChanged(StatusCode.Disconnected, "Disconnected by unknown error");
                        break;
                }

                _listener.OnClose();
            }

            if (_sendAckBackgroundThread != null)
            {
                _sendAckBackgroundThread.Abort();
                _sendAckBackgroundThread = null;
            }

            LogMessageRecevied = null;

            _isAckThreadRunning = false;
        }

        public bool FetchServerTimestamp()
        {
            if (_udpSocket.State != SocketState.Connected)
                return false;

            return EnqueueCommand(CommandType.SNTP, null, 0, false);
        }

        /// <summary>
        /// Called by a receive thread.
        /// </summary>
        void INetworkBroker.OnReceive(IByteBuffer buffer)
        {
            uint currentTime = EnvironmentTimer.GetTickCount();
            int bufferCount = buffer.Count;
            
            CommandType ct = (CommandType)buffer.ReadByte();

            if (ct != CommandType.None)
            {
                return;
            }

            //peerID (4 bytes)
            int peerID = buffer.ReadInt();

            if (_peerId > 0 && peerID != _peerId)
            {
                Log(LogLevel.Error, "A assigned peerID [{0}] is different from recevied peerID [{1}] from server", peerID, _peerId);
                Disconnect(DisconnectReason.InvalidConnection, false);
            }

            //timestamp of sent time (8 byte)
            uint serverSentTime = (uint)buffer.ReadLong();

            //the number of commands in a incoming buffer (2 byte)
            short commandCount = buffer.ReadShort();

            //crc value (8 byte)
            long crc = buffer.ReadLong();

            if (IsCrcEnabled)
            {
                int writeIndex = buffer.WriteIndex;

                buffer.WriteIndex = buffer.ReadIndex - (int)Lengths.Crc;
                buffer.WriteLong(0);
                buffer.WriteIndex = writeIndex;

                long calc = buffer.CalculateCrc();

                if (crc != calc)
                {
                    if (_allowStatistics)
                        _statistics.ErrorCrc();

                    return;
                }
            }

            for (int i = 0; i < commandCount; i++)
            {
                IncomingCommand command = new IncomingCommand(buffer, currentTime, serverSentTime);

                if (command.IsReliable)
                {
                    SendAckFromCommand(command);
                }

                if (command.Type == CommandType.Acknowledge)
                {
                    AckHandler(command);
                }
                else
                {
                    EnqueueRenderingQueue(() => ExecuteReceiveCommand(command));
                }
            }

            if (_allowStatistics)
            {
                _statistics.ReceiveBytes(bufferCount, currentTime);
                _statistics.ReceiveMtu(serverSentTime);
                _statistics.ReceiveIncomingCommand(commandCount);
            }
        }

        private void SendAckBackground()
        {
            while (_udpSocket.State != SocketState.Disconnected && _isAckThreadRunning)
            {
                if (_udpSocket.State == SocketState.Connecting)
                {
                    TryToConnect();
                }
                else
                {
#if PERF
                    break;
#else
                    SendAck();
#endif
                }

                Thread.Sleep(1000);
            }

            _isAckThreadRunning = false;
        }

        private Timer _timer;
        private void TryToConnectAsync()
        {
            _timer = new Timer(_ =>
            {
                if (_udpSocket.State == SocketState.Connecting)
                {
                    TryToConnect();
                }
                else if (_udpSocket.State == SocketState.Connected)
                {
                    if (_timer != null)
                        _timer.Dispose();
                }
            }, null, 0, DisconnectTimeout);
        }

        private void TryToConnect()
        {
            if (_connectionBroker.SentCount >= SentCountAllowance)
            {
                Disconnect(DisconnectReason.ConnectionFailed, false);
                return;
            }

            if (_connectionBroker.SeningTime + DisconnectTimeout < EnvironmentTimer.GetTickCount())
            {
                _connectionBroker.SeningTime = (int)EnvironmentTimer.GetTickCount();

                _udpSocket.Send(_connectionBroker.CreateConnectCommand(
                    ChannelCount,
                    MTU,
                    DataSerializer.Version,
                    DisconnectTimeout,
                    IsCrcEnabled,
                    _cipher.PublicKey,
                    _connectionBroker.SeningTime));
            }
        }

        /// <summary>
        /// Fetch a payload. If a incoming command is invalid, return null.
        /// </summary>
        /// <returns>A payload in a incoming command.</returns>
        private IByteBuffer FetchPayload(IncomingCommand command)
        {
            if (false == command.IsPayloadAvailable())
                return null;

            // The length of InitialResponse is 2. Thus payload length should be bigger then 2.
            if (command.GetPayload().Length < 2)
            {
                Log(LogLevel.Error, "Incoming UDP data is too short; Length [{0}] ", command.GetPayload().Length);
                return null;
            }

            if (command.IsEncrypted)
            {
                try
                {
                    command.SetPayload(_cipher.Decrypt(command.GetPayload()));
                }
                catch (Exception)
                {
                    Log(LogLevel.Error, "Invalid encrypted data");
                    return null;
                }
            }

            var payload = ByteBufferFactory.NewBuffer(command.GetPayload());

            byte serializerVersion = payload.ReadByte();

            if (serializerVersion != DataSerializer.Version)
            {
                Log(LogLevel.Error, "Unknown serializer Version");
                return null;
            }

            return payload;
        }
        
        private void ResetQueues()
        {
            _channels.ClearAll();
        }

        private void SendBuffer(int commandCount, long currentSendingTime)
        {
            if (commandCount <= 0)
                return;
            
            EncapsulateMtu(commandCount, currentSendingTime);

            try
            {
                _udpSocket.Send(_udpBuffer, _udpBufferIndex);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "Exception occurred during sending data : {0}", ex.ToString());
            }
        }

        private void EncapsulateMtu(int commandCount, long currentSendingTime)
        {
            int offset = 0;

            ByteWrite.SetByte(_udpBuffer, ref offset, (byte)CommandType.None);
            ByteWrite.SetInt(_udpBuffer, ref offset, _peerId);
            ByteWrite.SetLong(_udpBuffer, ref offset, currentSendingTime);
            ByteWrite.SetShort(_udpBuffer, ref offset, (short)commandCount);

            Array.Clear(_udpBuffer, offset, (int)Lengths.Crc);

            if (IsCrcEnabled)
            {
                long crc = _udpBuffer.CalculateCrc(_udpBufferIndex);
                ByteWrite.SetLong(_udpBuffer, ref offset, crc);
            }
        }

        private int SerializeQueue(SendQueueBase sendQueue, uint currentSendingTime)
        {
            var commands = sendQueue.DequeueSendableCommands(_udpBufferIndex, _udpBuffer.Length);
            var udpCommandCount = commands.Count;
            for (int i = 0; i < commands.Count; i++)
            {
                _udpBufferIndex = commands[i].Serialize(_udpBuffer, _udpBufferIndex);
                if (commands[i].IsReliable)
                {
                    EnqueueSentCommand(commands[i], currentSendingTime);
                }
            }
            return udpCommandCount;
        }
        
        private int RemainingAckCommands()
        {
            int count = 0;

            foreach (UdpChannel ch in _channels)
            {
                count += ch.AckQueue.RemainingCommandCount;
            }

            return count;
        }

#region Dispose Pattern

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _disposed = true;

                LogMessageRecevied = null;

                if (_udpSocket != null && _udpSocket.State != SocketState.Disconnected)
                {
                    _udpSocket.Dispose();
                }
            }
        }

#endregion Dispose Pattern
    }
}