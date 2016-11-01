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
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Haste.Network
{
    internal class UdpSocket : IDisposable
    {
        private INetworkBroker _broker;
        private Thread _receiveThread;
        private UdpClient _client = null;

        private int _state = (int)SocketState.Disconnected;
        private bool _disposed = false;

        private short _mtuSize;

        private int _startPort = -1;
        private int _endPort = -1;

        private readonly int _disconnectTimeout;

        public UdpSocket(short mtuSize, int disconnectTimeout)
        {
            _mtuSize = mtuSize;
            _disconnectTimeout = disconnectTimeout;
        }

        internal void SetBindPort(int startPort, int endPort)
        {
            _startPort = startPort;
            _endPort = endPort;
        }

        internal void SetBroker(INetworkBroker broker, SocketState state)
        {
            Interlocked.Exchange(ref _broker, broker);
            Interlocked.Exchange(ref _state, (int)state);
        }

        internal SocketState State
        {
            get { return (SocketState)_state; }
            private set { Interlocked.Exchange(ref _state, (int)value); }
        }

        internal void Disconnecting()
        {
            State = SocketState.Disconnecting;
        }

        private void Close()
        {
            if (State == SocketState.Disconnected)
                return;

            State = SocketState.Disconnected;

            try
            {
                if (_client != null)
                {
                    _client.Close();
                    _client = null;
                }

                if (_receiveThread != null)
                {
                    _receiveThread.Abort();
                    _receiveThread = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        internal bool Prepare(IPEndPoint remoteEndPoint)
        {
            if (State != SocketState.Connecting)
            {
                _broker.Log(LogLevel.Error, "Socket is already in use");
                return false;
            }

            try
            {
                if (_startPort <= -1 || _endPort <= -1)
                {
                    _client = new UdpClient(0, remoteEndPoint.AddressFamily);
                }
                else
                {
                    _client = new UdpClient();
                    BindAvailablePort(remoteEndPoint.AddressFamily, _startPort, _endPort);
                }

                _client.Client.SendTimeout = _disconnectTimeout;
                _client.Client.ReceiveTimeout = _disconnectTimeout;
            }
            catch (Exception ex)
            {
                _broker.Log(LogLevel.Error, ex.ToString());
                Close();
                return false;
            }

            if (_receiveThread != null)
                _receiveThread.Abort();

#if PERF
            _client.BeginReceive(ReceiveAsync, null);
#else
            _receiveThread = new Thread(ReceiveFromLoop);
            _receiveThread.Name = "HasteReceiveThread";
            _receiveThread.IsBackground = true;

            _receiveThread.Start();
            _broker.Log(LogLevel.Debug, "Start receive thread : {0}", _receiveThread.Name);
#endif
            return true;
        }

        private void BindAvailablePort(AddressFamily remoteAddressFamily, int startPort, int endPort)
        {
            IPAddress ipAddress = remoteAddressFamily == AddressFamily.InterNetworkV6
                ? IPAddress.IPv6Any : IPAddress.Any;

            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
            for (int port = startPort; port < endPort; port++)
            {
                try
                {
                    _client.Client.Bind(new IPEndPoint(ipAddress, port));
                    return;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        internal bool Send(byte[] data)
        {
            return Send(data, data.Length);
        }

        internal bool Send(byte[] data, int length)
        {
            if (State == SocketState.Disconnected)
            {
                _broker.Log(LogLevel.Debug, "SocketState is Disconnected!");
                return false;
            }

            try
            {
                if (_client != null)
                {
                    return _client.Send(data, length, _broker.RemoteEndPoint) == length;
                }
                return false;
            }
            catch (Exception ex)
            {
                _broker.Log(LogLevel.Error, "Fail to Send Data : {0}", ex.ToString());
                return false;
            }
        }

        private void ReceiveAsync(IAsyncResult res)
        {
            if (State == SocketState.Disconnected)
                return;

            byte[] buf = null;

            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);
            if (_client == null)
                return;

            buf = _client.EndReceive(res, ref remoteIpEndPoint);

            if (buf == null || buf.Length < UdpPeer.MtuHeaderLength || buf.Length > _mtuSize)
            {
                throw new Exception(string.Format("The length of received data[{0}] is out of range between {1} and {2}", buf.Length, UdpPeer.MtuHeaderLength, _mtuSize));
            }

            var buffer = ByteBufferFactory.NewBuffer(buf);

            _broker.OnReceive(buffer);

            if (_client == null)
                return;

            _client.BeginReceive(ReceiveAsync, null);
        }

        protected void ReceiveFromLoop()
        {
            while (State != SocketState.Disconnected)
            {
                try
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    byte[] buf = _client.Receive(ref remoteEndPoint);

                    if (buf == null || buf.Length < UdpPeer.MtuHeaderLength || buf.Length > _mtuSize)
                    {
                        throw new Exception(string.Format("The length of received data[{0}] is out of range between {1} and {2}", buf.Length, UdpPeer.MtuHeaderLength, _mtuSize));
                    }

                    var buffer = ByteBufferFactory.NewBuffer(buf);

                    _broker.OnReceive(buffer);
                }
                catch (SocketException ex)
                {
                    if (State != SocketState.Disconnecting && State != SocketState.Disconnected)
                        _broker.Log(LogLevel.Error, ex.ToString());

                    break;
                }
                catch (Exception ex)
                {
                    if (State != SocketState.Disconnecting && State != SocketState.Disconnected)
                        _broker.Log(LogLevel.Error, ex.ToString());

                    break;
                }
            }
        }

        #region Dispose Pattern

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (State != SocketState.Disconnected)
            {
                Close();
            }

            GC.SuppressFinalize(this);
        }

        #endregion Dispose Pattern
    }
}