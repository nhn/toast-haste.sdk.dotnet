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

using System;
using System.Net.Sockets;
using Haste.Data;
using Haste.Network;
using System.Net;
using System.Text;
using Haste.ByteBuffer;
using Haste.Messages;

namespace Haste
{
    public sealed class HastePeer : IDisposable 
    {
        private readonly Version _sdkVersion = new Version(1, 2, 3);

        private readonly INetworkPeer _peer;
        private readonly IListener _listener;

        public int PeerID { get { return _peer.PeerID; } }
        public uint ServerTime { get { return _peer.ServerTime; } }
        public uint RoundTripTime { get { return _peer.MeanOfRoundTripTime; } }
        public int MaxUnreliableCommands { get { return _peer.MaxUnreliableCommands; } }
        public bool IsCrcEnabled { get { return _peer.IsCrcEnabled; } }

        #region Network Statistics

        public NetStatistics Statistics
        {
            get { return _peer.Statistics; }
        }

        public int AckWaitQueueCount
        {
            get { return _peer.QueuedSentCommandCount; }
        }

        #endregion

        public HastePeer(byte[] customData, Version clientVersion, IListener listener, ProtocolType protocol, ConnectionConfig config)
        {
            _listener = listener;
            switch (protocol)
            {
                case ProtocolType.Udp:
                    _peer = new UdpPeer(customData, _sdkVersion, clientVersion, listener, config);
                    break;
                case ProtocolType.Tcp:
                    _peer = new TcpPeer(customData, clientVersion, listener);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unsupported protocol type {0}", protocol));
            }
        }

        public void NetworkUpdate()
        {
            if (_peer.SocketState != SocketState.Connected)
                return;

            // Read message from IPeer, and deserialize message.
            var buffers = _peer.ReadMessages();
            foreach (var buffer in buffers)
            {
                if (!HandlePayload(buffer))
                {
                    _peer.Disconnect(DisconnectReason.InvalidDataFormat, true);
                }
            }

            _peer.RunQueuedActions();
            _peer.FlushSendQueues();
        }

        public bool Connect(IPEndPoint remoteEndPoint)
        {
            return _peer.Connect(remoteEndPoint);
        }

        public bool SendRequestMessage(short requestCode, DataObject payload, SendOptions options)
        {
            return _peer.EnqueueRequest(requestCode, payload, options, MessageType.RequestMessage);
        }

        private bool HandlePayload(IByteBuffer payload)
        {
            if (payload == null) return false;

            MessageType messageType = (MessageType)payload.ReadByte(); //1

            switch (messageType)
            {
                case MessageType.InitialResponse:
                    short resultCode = payload.ReadShort();
                    short len = payload.ReadShort();

                    byte[] buf = new byte[len];
                    payload.ReadBytes(buf, 0, len);

                    string message = Encoding.UTF8.GetString(buf);

                    if (_listener != null)
                    {
                        _listener.OnStatusChanged(StatusCode.ServerConnected, message);
                    }

                    return true;
                case MessageType.ResponseMessage:
                    short code = payload.ReadShort();
                    DataObject parameters = payload.ToDataObject();

                    ResponseMessage response = new ResponseMessage(code, parameters);

                    if (_listener != null)
                    {
                        _listener.OnResponseMessage(response);
                    }
                    return true;
                case MessageType.EventMessage:
                    EventMessage eventData = new EventMessage(payload.ReadShort(), payload.ToDataObject());

                    if (_listener != null)
                    {
                        _listener.OnEventMessage(eventData);
                    }
                    return true;
                default:
                    return false;
            }
        }

        public void Disconnect(DisconnectReason reason = DisconnectReason.ClientDisconnect)
        {            
            _peer.Disconnect(reason, true);   
        }
        
        public void FetchServerTimestamp()
        {
            _peer.FetchServerTimestamp();
        }

        public event Action<LogLevel, string> LogMessageRecevied
        {
            add
            {
                _peer.LogMessageRecevied += value;
            }
            remove
            {
                _peer.LogMessageRecevied -= value;
            }
        }

        public void Dispose()
        {
            Disconnect();
            _peer.Dispose();
        }
    }
}
