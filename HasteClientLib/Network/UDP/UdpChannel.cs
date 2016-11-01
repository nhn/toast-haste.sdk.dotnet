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

using Haste.Network.Queues;

namespace Haste.Network
{
    internal class UdpChannel
    {
        private readonly ConnectionConfig _config;
        
        internal static readonly long InitialSequenceNumber = 0;

        internal byte ChannelNumber;

        internal long IncomingReliableSequenceNumber = InitialSequenceNumber;
        internal long IncomingUnreliableSequenceNumber = InitialSequenceNumber;
        internal long OutgoingReliableSequenceNumber = InitialSequenceNumber;
        internal long OutgoingUnreliableSequenceNumber = InitialSequenceNumber;

        private readonly ReceiveQueueBase _reliableReceiveQueue;
        private readonly ReceiveQueueBase _unreliableReceiveQueue;

        private readonly ReliableSendQueue _reliableSendQueue;
        private readonly UnreliableSendQueue _unreliableSendQueue;
        
        private AckSendQueue _ackQueue;

        public ReceiveQueueBase ReliableReceiveQueue { get { return _reliableReceiveQueue; } }

        public ReceiveQueueBase UnreliableReceiveQueue { get { return _unreliableReceiveQueue; } }

        public ReliableSendQueue ReliableSendQueue { get { return _reliableSendQueue; } }

        public UnreliableSendQueue UnreliableSendQueue { get { return _unreliableSendQueue; } }

        public AckSendQueue AckQueue { get { return _ackQueue; } }
        
        internal int MaxUnreliableCommandCount { get { return _config.MaxUnreliableCommands; } }

        internal UdpChannel(ConnectionConfig config, byte channelNumber, int initialCapacity)
        {
            _config = config;

            ChannelNumber = channelNumber;

            _reliableReceiveQueue = new ReliableReceiveQueue(this, initialCapacity);
            _unreliableReceiveQueue = new UnreliableReceiveQueue(this, initialCapacity);

            _reliableSendQueue = new ReliableSendQueue(this, initialCapacity);
            _unreliableSendQueue = new UnreliableSendQueue(this, initialCapacity);

            _ackQueue = new AckSendQueue(this, initialCapacity);
        }

        internal int RemainingOutgoingCommands()
        {
            return ReliableSendQueue.RemainingCommandCount + UnreliableSendQueue.RemainingCommandCount;
        }

        internal void ClearAll()
        {
            lock (this)
            {
                _reliableReceiveQueue.ClearCommands();
                _unreliableReceiveQueue.ClearCommands();
                _reliableSendQueue.ClearCommands();
                _unreliableSendQueue.ClearCommands();

                IncomingReliableSequenceNumber = InitialSequenceNumber;
                IncomingUnreliableSequenceNumber = InitialSequenceNumber;
                OutgoingReliableSequenceNumber = InitialSequenceNumber;
                OutgoingUnreliableSequenceNumber = InitialSequenceNumber;

                _ackQueue.ClearCommands();
            }
        }
    }
}