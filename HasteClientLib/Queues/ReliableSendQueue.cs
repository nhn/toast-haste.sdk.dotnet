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
using System.Threading;

namespace Haste.Network.Queues
{
    internal class ReliableSendQueue : SendQueueBase
    {
        private readonly List<OutgoingCommand> _sentCommands;
        private int _resendingTimeout;

        internal int RemainingSentCommandCount { get { return _sentCommands.Count; } }

        public ReliableSendQueue(UdpChannel channel, int initialCapacity) : base(channel, initialCapacity)
        {
            _sentCommands = new List<OutgoingCommand>(initialCapacity);
        }
        
        public override bool EnqueueOutgoingCommand(OutgoingCommand command)
        {
            if (UdpChannel.InitialSequenceNumber == command.ReliableSequenceNumber)
            {
                command.ReliableSequenceNumber = ++Channel.OutgoingReliableSequenceNumber;
            }

            Commands.Enqueue(command);

            return true;
        }
        
        internal bool EnqueueSentCommand(OutgoingCommand command, uint currentSendingTime, uint newRtt, uint disconnectTimeout)
        {
            command.SentTime = currentSendingTime;
            command.SendAttempts++; // 송신 횟수
            // 재전송을 위한 평균 RTT + 평균 분산 값
            command.RoundTripTimeout = newRtt * command.SendAttempts;

            lock (_sentCommands)
            {
                // sentReliableCommands 비어 있을 경우, 현재 command를 기준으로 재전송 시간을 설정한다.
                OutgoingCommand oc = _sentCommands.Count > 0 ? _sentCommands[0] : command;
                Interlocked.Exchange(ref _resendingTimeout, (int)(oc.SentTime + command.RoundTripTimeout));

                if (command.SendAttempts <= 1)
                {
                    command.SendingTimeout = command.SentTime + disconnectTimeout;
                    _sentCommands.Add(command);
                }
            }

            return true;
        }
        
        public OutgoingCommand ReceiveAck(IncomingCommand inCommand)
        {
            return RemoveSentReliableCommand(inCommand);
        }

        internal OutgoingCommand RemoveSentReliableCommand(IncomingCommand inCommand)
        {
            lock (_sentCommands)
            {
                //채널과 reliableSequenceNumber가 동일한 command를 삭제한다.
                OutgoingCommand command = _sentCommands.Find(o => o != null &&
                                                                 o.ReliableSequenceNumber == inCommand.AckReceivedReliableSequenceNumber &&
                                                                 o.Channel == inCommand.Channel);

                if (command != null)
                {
                    _sentCommands.Erase(command);

                    if (_sentCommands.Count > 0)
                    {
                        Interlocked.Exchange(ref _resendingTimeout, (int)(_sentCommands[0].SentTime + _sentCommands[0].RoundTripTimeout));
                    }
                }
                return command;
            }
        }
        
        /// <summary>
        /// Enqueue resending commands to outgoing queue in peer. If sent commands have timeout command, return false.
        /// </summary>
        internal Queue<OutgoingCommand> FetchResendingCommands(uint currentTime, int sentCountAllowance)
        {
            Queue<OutgoingCommand> resendingQueue = new Queue<OutgoingCommand>();

            // Reliable Command 송신 후, Ack가 수신되지 않아 재 전송.
            if (currentTime > _resendingTimeout && _sentCommands.Count > 0)
            {
                lock (_sentCommands)
                {
                    resendingQueue.Clear();

                    foreach (var command in _sentCommands)
                    {
                        resendingQueue.Enqueue(command);
                    }
                }
            }
            
            return resendingQueue;
        }

    }
}
