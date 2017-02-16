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
using Haste.Messages;

namespace Haste
{
    public class NetworkConnection : IListener
    {
        public static readonly short DefaultChannelCount = 5;
        public static readonly short DefaultMTUSize = 1300;
        public static readonly int DefaultPingInterval = 500;
        public static readonly int DefaultPingDisconnectionTimeout = 3000;

        private HastePeer _peer;

        private ConnectionConfig _config;

        private int _netStatus;

        /// <summary>
        /// Gets the status about network.
        /// </summary>
        public NetStates NetStatus
        {
            get { return (NetStates)_netStatus; }
        }

        /// <summary>
        /// Gets PeerID assigned by the server if connection is established or -1 if no connection.
        /// </summary>
        public int PeerId
        {
            get { return _peer == null ? -1 : _peer.PeerID; }
        }

        /// <summary>
        /// Occurs when the status is changed.
        /// </summary>
        public event Action<StatusCode, string> StatusChanged;

        /// <summary>
        /// Occurs when the log is received.
        /// </summary>
        public event Action<LogLevel, string> LogMessageReceived;

        /// <summary>
        /// Occurs when the response message is received.
        /// </summary>
        public event Action<ResponseMessage> ResponseReceived;

        /// <summary>
        /// Occurs when the event message is received.
        /// </summary>
        public event Action<EventMessage> EventReceived;

        public NetworkConnection()
        {
            Interlocked.Exchange(ref _netStatus, (int)NetStates.Disconnected);

            _config = new ConnectionConfig()
            {
                ChannelCount = DefaultChannelCount,
                MtuSize = DefaultMTUSize,
                PingInterval = DefaultPingInterval,
                DisconnectionTimeout = DefaultPingDisconnectionTimeout,
            };
        }

        /// <summary>
        /// Update packets from queues. This method should be called periodically.
        /// </summary>
        public void NetworkUpdate()
        {
            if (_peer != null)
                _peer.NetworkUpdate();
        }

        private const int MinChannelCount = 1;
        private const int MaxChannelCount = 100;

        private const int MinMtuSize = 400;
        private const int MaxMtuSize = 1400;

        private const int MaxPingInterval = 300;

        /// <summary>
        /// Set configuration abount connection. Reference to <see cref="ConnectionConfig"/>
        /// </summary>
        /// <param name="config">Connection configuration</param>
        public void Configure(ConnectionConfig config)
        {
            if (config.ChannelCount < MinChannelCount || config.ChannelCount > MaxChannelCount)
                throw new ArgumentOutOfRangeException(string.Format("The count of channel must be set between {0} and {1}.", MinChannelCount, MaxChannelCount));

            if (config.MtuSize < MinMtuSize || config.MtuSize > MaxMtuSize)
                throw new ArgumentOutOfRangeException(string.Format("The size of MTU must be set between {0} and {1}.", MinMtuSize, MaxMtuSize));

            if (config.PingInterval < MaxPingInterval)
                throw new ArgumentOutOfRangeException(string.Format("The interval of ping must be less than {0}", MaxPingInterval));

            if (config.DisconnectionTimeout < config.PingInterval * 3)
                throw new ArgumentOutOfRangeException("The timeout for disconnection must be less than three times of ping interval.");

            _config = config;
        }

        /// <summary>
        /// Connect to remote server. (TCP protocol is not supported yet)
        /// For supporting DNS64/NAT64, pass address after querying domain to DNS Server, using <see cref="QueryDns"/>.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="version"></param>
        /// <param name="customData"></param>
        /// <param name="protocol"></param>
        public void Connect(IPEndPoint remoteEndPoint, Version version, byte[] customData, ProtocolType protocol = ProtocolType.Udp)
        {
            if (protocol == ProtocolType.Tcp)
            {
                throw new NotSupportedException("TCP protocol is not supported yet!");
            }

            if (_peer != null)
            {
                _peer.Dispose();
            }

            _peer = new HastePeer(customData, version, this, protocol, _config);

            _peer.LogMessageRecevied += OnReceivedLogMessage;

            _peer.Connect(remoteEndPoint);

            Interlocked.Exchange(ref _netStatus, (int)NetStates.Connecting);
        }

        private void OnReceivedLogMessage(LogLevel level, string message)
        {
            if (LogMessageReceived != null)
                LogMessageReceived(level, message);
        }

        /// <summary>
        /// Disconnect from the remote server.
        /// </summary>
        public void Disconnect()
        {
            if (_netStatus == (int)NetStates.Disconnected)
                return;

            if (_peer != null)
            {
                Interlocked.Exchange(ref _netStatus, (int)NetStates.Disconnected);
                _peer.Dispose();
            }
        }

        /// <summary>
        /// Get server time. If you want to get the relatively exact server time, call <see cref="FetchServerTimestamp"/> before getting the server time.
        /// </summary>
        public uint ServerTime
        {
            get { return _peer != null ? _peer.ServerTime : 0; }
        }

        /// <summary>
        /// Get the mean of round trip time.
        /// </summary>
        public uint RoundTripTime
        {
            get { return _peer != null ? _peer.RoundTripTime : 0; }
        }

        #region NetworkInfo

        /// <summary>
        /// Get the statistics about network.
        /// </summary>
        public NetStatistics Statistics
        {
            get { return _peer.Statistics; }
        }

        /// <summary>
        /// Get the count of a sent packet that is waiting ack.
        /// </summary>
        public int AckWaitQueueCount
        {
            get { return _peer.AckWaitQueueCount; }
        }

        #endregion NetworkInfo

        #region Implementation of IListener

        void IListener.OnStatusChanged(StatusCode statusCode, string message)
        {
            if (StatusChanged != null)
            {
                StatusChanged(statusCode, message);
            }

            switch (statusCode)
            {
                case StatusCode.ServerConnected:
                    Interlocked.Exchange(ref _netStatus, (int)NetStates.Connected);
                    _peer.FetchServerTimestamp();
                    break;
                case StatusCode.FailedToConnect:
                case StatusCode.Disconnected:
                case StatusCode.FailedToReceive:
                case StatusCode.FailedToSend:
                    Disconnect();
                    break;
            }
        }

        void IListener.OnResponseMessage(ResponseMessage response)
        {
            if (ResponseReceived != null)
                ResponseReceived(response);
        }

        void IListener.OnEventMessage(EventMessage eventData)
        {
            if (EventReceived != null)
                EventReceived(eventData);
        }

        void IListener.OnClose()
        {
            Interlocked.Exchange(ref _netStatus, (int)NetStates.Disconnected);

            EventReceived = null;
            ResponseReceived = null;
            LogMessageReceived = null;
            StatusChanged = null;
        }

        #endregion //Implementation of IListener

        /// <summary>
        /// Send the request message.
        /// </summary>
        /// <param name="requestCode">The code of a request message.</param>
        /// <param name="payload">The payload of a request message.</param>
        /// <param name="options">The option about network of a request message.</param>
        /// <returns></returns>
        public bool SendRequestMessage(short requestCode, DataObject payload, SendOptions options)
        {
            if (_peer != null)
            {
                return _peer.SendRequestMessage(requestCode, payload, options);
            }

            return false;
        }

        /// <summary>
        /// Send a SNTP packet for fetching the server time.
        /// </summary>
        public void FetchServerTimestamp()
        {
            _peer.FetchServerTimestamp();
        }

        /// <summary>
        /// Query to a dns server, and return a ip address. If not found a address, this method returns null.
        /// </summary>
        public static IPEndPoint QueryDns(string domain, short port)
        {
            var domainAddresses = Dns.GetHostEntry(domain).AddressList;

            foreach (var addr in domainAddresses)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return new IPEndPoint(addr, port);
                }
            }

            foreach (var addr in domainAddresses)
            {
                if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return new IPEndPoint(addr, port);
                }
            }

            return null;
        }
    }
}
