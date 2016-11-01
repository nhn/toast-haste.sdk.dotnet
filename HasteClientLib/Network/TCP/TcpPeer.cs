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
using System.Collections.Generic;
using System.Net;
using Haste.ByteBuffer;
using Haste.Security;

namespace Haste.Network
{
    internal class TcpPeer : INetworkPeer
    {
        private TcpSocket _socket;

        public TcpPeer(byte[] customData, Version version, IListener listener)
        {
        }

        public event Action<LogLevel, string> LogMessageRecevied;

        public LogLevel DebugLevel { get; set; }

        public int PeerID
        {
            get { throw new NotImplementedException(); }
        }

        public int QueuedSentCommandCount
        {
            get { throw new NotImplementedException(); }
        }

        public uint ServerTime
        {
            get { throw new NotImplementedException(); }
        }

        public SocketState SocketState
        {
            get { throw new NotImplementedException(); }
        }

        public NetStatistics Statistics
        {
            get { throw new NotImplementedException(); }
        }

        public bool Connect(IPEndPoint remoteEndPoint)
        {
            throw new NotImplementedException();
        }

        public bool IsCrcEnabled 
        { 
            get 
            {
                throw new NotImplementedException();
            } 
        }

        public void Disconnect(DisconnectReason reason, bool sending)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool EnqueueRequest(short code, DataObject data, SendOptions options, MessageType messageType)
        {
            throw new NotImplementedException();
        }

        public bool FetchServerTimestamp()
        {
            throw new NotImplementedException();
        }

        public void RunQueuedActions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IByteBuffer> ReadMessages()
        {
            throw new NotImplementedException();
        }

        public void FlushSendQueues()
        {
            throw new NotImplementedException();
        }

        public uint MeanOfRoundTripTime
        {
            get { throw new NotImplementedException(); }
        }


        public Cipher Cipher
        {
            get { throw new NotImplementedException(); }
        }
        
        public int MaxUnreliableCommands
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}