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

namespace Haste.Network.Queues
{
    class ReliableReceiveQueue : ReceiveQueueBase
    {
        internal ReliableReceiveQueue(UdpChannel channel, int initialCapacity)
            : base(channel, initialCapacity)
        {
        }

        public override bool TryProcessCommand(out IncomingCommand command)
        {
            if (!Commands.TryGetValue(Channel.IncomingReliableSequenceNumber + 1, out command))
                return false;

            if (command.Type == CommandType.Fragmented)
                return ProcessFragmentCommand(command);

            Channel.IncomingReliableSequenceNumber = command.ReliableSequenceNumber;
            return Commands.Remove(command.ReliableSequenceNumber);
        }

        private bool ProcessFragmentCommand(IncomingCommand command)
        {
            if (command.FragmentsRemaining > 0)
                return false;

            byte[] array = new byte[command.TotalLength];

            for (long i = command.StartSequenceNumber; i < command.StartSequenceNumber + command.FragmentCount; i++)
            {
                if (!Commands.ContainsKey(i))
                {
                    throw new Exception(string.Format("Failed to found {0} command.", i));
                }

                IncomingCommand current = Commands[i];
                byte[] currentPayload = current.GetPayload();

                Buffer.BlockCopy(currentPayload, 0, array, current.FragmentOffset, currentPayload.Length);

                Commands.Remove(current.ReliableSequenceNumber);
            }

            command.SetPayload(array);

            //command.Size = (Int32)(12 * command.FragmentCount + command.TotalLength);
            Channel.IncomingReliableSequenceNumber = (uint)(command.ReliableSequenceNumber + command.FragmentCount - 1);

            return true;
        }

        public override bool EnqueueIncomingCommand(IncomingCommand command)
        {
            if (command.ReliableSequenceNumber <= Channel.IncomingReliableSequenceNumber)
            {
                //Duplicated command, already received command.
                return false;
            }

            if (Contains(command.ReliableSequenceNumber))
                return false;

            AddCommand(command.ReliableSequenceNumber, command);
            
            return true;
        }

        public void ReceiveFragmentCommand(IncomingCommand command)
        {
            if (command.ReliableSequenceNumber == command.StartSequenceNumber)
            {
                command.FragmentsRemaining--;
                long num = command.StartSequenceNumber + 1;

                while (command.FragmentsRemaining > 0 && num < command.StartSequenceNumber + command.FragmentCount)
                {
                    if (Contains(num++))
                    {
                        command.FragmentsRemaining--;
                    }
                }
            }
            else
            {
                if (Contains(command.StartSequenceNumber))
                {
                    IncomingCommand remain = this[command.StartSequenceNumber];
                    remain.FragmentsRemaining--;
                }
            }
        }
    }
}
