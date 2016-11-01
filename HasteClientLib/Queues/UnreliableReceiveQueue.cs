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

using System.Collections.Generic;

namespace Haste.Network.Queues
{
    class UnreliableReceiveQueue : ReceiveQueueBase
    {
        internal UnreliableReceiveQueue(UdpChannel channel, int initialCapacity)
            : base(channel, initialCapacity)
        {
        }

        public override bool TryProcessCommand(out IncomingCommand command)
        {
            command = null;
            Queue<long> removeSequenceNumbers = new Queue<long>();

            if (Commands.Count > 0)
            {
                long lastSequenceNumber = long.MaxValue;

                foreach (var unreliableSequenceNumber in Commands.Keys)
                {
                    IncomingCommand unreliableCommand = Commands[unreliableSequenceNumber];

                    if (IsDroppable(unreliableCommand.ReliableSequenceNumber, unreliableSequenceNumber))
                    {
                        removeSequenceNumbers.Enqueue(unreliableSequenceNumber); //Drop
                    }
                    else if (unreliableSequenceNumber < lastSequenceNumber)
                    {
                        if (unreliableCommand.ReliableSequenceNumber <= Channel.IncomingReliableSequenceNumber)
                        {
                            //마지막 Dispatched reliable command 이후에 전송한 unreliable command 중 제일 작인 seq번호
                            lastSequenceNumber = unreliableSequenceNumber;
                        }
                    }
                }

                //queue에 해당되는 제거
                RemoveCommands(removeSequenceNumbers);

                if (lastSequenceNumber < int.MaxValue)
                {
                    command = Commands[lastSequenceNumber];

                    Commands.Remove(command.UnreliableSequenceNumber);
                    Channel.IncomingUnreliableSequenceNumber = command.UnreliableSequenceNumber;

                    return true;
                }
            }
            return false;
        }

        private bool IsDroppable(long reliableSequenceNumber, long unreliableSequenceNumber)
        {
            bool isDelayed = unreliableSequenceNumber < Channel.IncomingUnreliableSequenceNumber ||
                         reliableSequenceNumber < Channel.IncomingReliableSequenceNumber;
            bool isExceed = Channel.MaxUnreliableCommandCount > 0 && Commands.Count > Channel.MaxUnreliableCommandCount;
            return isDelayed || isExceed;
        }

        private void RemoveCommands(IEnumerable<long> sequenceNumbers)
        {
            foreach (var seqNum in sequenceNumbers)
            {
                Commands.Remove(seqNum);
            }
        }

        public override bool EnqueueIncomingCommand(IncomingCommand command)
        {
            if (command.ReliableSequenceNumber < Channel.IncomingReliableSequenceNumber ||
                command.UnreliableSequenceNumber <= Channel.IncomingUnreliableSequenceNumber ||
                Contains(command.UnreliableSequenceNumber))
                return false;

            AddCommand(command.UnreliableSequenceNumber, command);
            
            return true;
        }
    }
}
