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
using System.Threading;

namespace Haste.Network
{
    internal partial class UdpPeer
    {
        private void AckHandler(IncomingCommand command)
        {
            uint currentRoundTripTime = command.TimestampOfReceive - command.AckReceivedSentTime;

            // Find a channel that the command came, and remove a sent command.
            UdpChannel channel = _channels[command.Channel];
            OutgoingCommand reliableCommand = channel.ReliableSendQueue.ReceiveAck(command);

            //if reliableCommand is null, current command is already received.
            if (reliableCommand == null)
                return;

            if (reliableCommand.CommandType == CommandType.SNTP)
            {
                ProcessSntp(command, currentRoundTripTime);
            }

            _roundTripTime.Update((int)currentRoundTripTime);

            if (_allowStatistics)
            {
                _statistics.UpdateRoundTripTime(_roundTripTime);
                _statistics.ReceiveAck(command.TimestampOfReceive);
            }
        }

        private void ProcessSntp(IncomingCommand command, uint currentRoundTripTime)
        {
            uint currentServerTime = 0;

            if (_roundTripTime.MeanOfRoundTripTime > currentRoundTripTime)
            {
                currentServerTime = (command.ServerSentTime + (currentRoundTripTime >> 1));
            }
            else
            {
                currentServerTime = (command.ServerSentTime + ((uint)_roundTripTime.MeanOfRoundTripTime >> 1));
            }

            Interlocked.Exchange(ref _serverTimeOffset, (int)(currentServerTime - command.TimestampOfReceive));

            if (_listener != null)
            {
                _listener.OnStatusChanged(StatusCode.ServerTimeAvailable, "Available server time");
            }
        }

        private void SendAckFromCommand(IncomingCommand command)
        {
            bool sendable;

            UdpChannel channel = _channels[command.Channel];
            lock (channel.AckQueue)
            {
                sendable = channel.AckQueue.EnqueueOutgoingCommand(command.CreateAck());
            }

            if (sendable)
            {
                SendAck();
            }
        }

        private int SerializeAck(uint currentTime)
        {
            for (int i = 0; i < _channels.Count; i++)
            {
                lock (_channels[i].AckQueue)
                {
                    if (_channels[i].AckQueue.RemainingCommandCount > 0)
                        return SerializeQueue(_channels[i].AckQueue, currentTime);
                }
            }

            return 0;
        }

        private void SendAck()
        {
            do
            {
                lock (_udpBuffer)
                {
                    Array.Clear(_udpBuffer, 0, _udpBuffer.Length);

                    _udpBufferIndex = MtuHeaderLength;

                    uint currentTime = EnvironmentTimer.GetTickCount();

                    int commandCountToSend = SerializeAck(currentTime);

                    if (!EnqueueResendingCommands(currentTime)) return;

                    //Ping 전송이 필요한지 판단하여 송신
                    if (_udpSocket.State == SocketState.Connected &&
                        currentTime - _timestampOfLastReliableSend > PingInterval)
                    {
                        OutgoingCommand command = new OutgoingCommand(CommandType.Ping, 0);
                        EnqueueOutgoingCommand(command);

                        // Ping이 Reliable command이기 때문에, Reliable command들을 Serialize 한다.
                        for (int i = 0; i < _channels.Count; i++)
                        {
                            commandCountToSend += SerializeQueue(_channels[i].ReliableSendQueue, currentTime);
                        }

                        _timestampOfLastReliableSend = currentTime;
                    }

                    SendBuffer(commandCountToSend, currentTime);
                }
            } while (RemainingAckCommands() > 0);
        }
    }
}
