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
using Haste.Network;
using Haste.Security;
using System;
using System.Collections.Generic;
using System.Net;
using Haste.ByteBuffer;

namespace Haste
{
    public interface INetworkPeer : IDisposable
    {
        event Action<LogLevel, string> LogMessageRecevied;

        Cipher Cipher { get; }

        int PeerID { get; }

        int QueuedSentCommandCount { get; }

        uint ServerTime { get; }

        SocketState SocketState { get; }

        NetStatistics Statistics { get; }

        uint MeanOfRoundTripTime { get; }

        int MaxUnreliableCommands { get; }

        bool Connect(IPEndPoint remoteEndPoint);

        void Disconnect(DisconnectReason reason, bool sending);

        bool EnqueueRequest(short code, DataObject data, SendOptions options, MessageType messageType);

        bool FetchServerTimestamp();

        void RunQueuedActions();

        IEnumerable<IByteBuffer> ReadMessages();

        void FlushSendQueues();

        bool IsCrcEnabled { get; }
    }
}