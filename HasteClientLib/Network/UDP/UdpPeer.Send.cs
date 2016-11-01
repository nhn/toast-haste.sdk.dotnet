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
using Haste.Data;
using Haste.Messages;
using Haste.Network.Queues;

namespace Haste.Network
{
    internal partial class UdpPeer
    {
        public void FlushSendQueues()
        {
            if (_udpSocket.State == SocketState.Disconnected)
                return;

            int remainingCommands;
            do
            {
                remainingCommands = 0;
                lock (_udpBuffer)
                {
                    Array.Clear(_udpBuffer, 0, _udpBuffer.Length);

                    _udpBufferIndex = MtuHeaderLength;

                    uint currentTime = EnvironmentTimer.GetTickCount();

                    // Sends ack for received reliable commands (1st priority)
                    int commandCountToSend = SerializeAck(currentTime);

                    if (!EnqueueResendingCommands(currentTime)) return;
#if PERF
                    // Needs to send a ping command because a ack thread is not running in test client.
                    if (currentTime - _timestampOfLastReliableSend > PingInterval)
                        EnqueueOutgoingCommand(new OutgoingCommand(CommandType.Ping, 0));
#endif
                    // Send commands
                    int totalReliableCommand = 0;
                    int totalunReliableCommand = 0;

                    for (int i = 0; i < _channels.Count; i++)
                    {
                        totalReliableCommand += SerializeQueue(_channels[i].ReliableSendQueue, currentTime);
                        totalunReliableCommand += SerializeQueue(_channels[i].UnreliableSendQueue, currentTime);

                        remainingCommands += _channels[i].RemainingOutgoingCommands();
                    }
                    commandCountToSend += (totalReliableCommand + totalunReliableCommand);

                    //if udpCommandCount is more than zero, there is commands to send.
                    SendBuffer(commandCountToSend, currentTime);

                    if (totalReliableCommand > 0)
                    {
                        _timestampOfLastReliableSend = currentTime;
                    }
                }
            } while (remainingCommands > 0);
        }

        public bool EnqueueRequest(short code, DataObject parameters, SendOptions options, MessageType messageType)
        {
            if (_udpSocket.State != SocketState.Connected)
            {
                if (_listener != null)
                {
                    string message = string.Format("Cannot send op: {0}, PeerState : {1}", code, _udpSocket.State);
                    _listener.OnStatusChanged(StatusCode.FailedToSend, message);
                }

                return false;
            }

            if (options == null)
            {
                if (_listener != null)
                {
                    string message = string.Format("Cannot send op: {0}, PeerState : {1}, SendOptions : null", code, _udpSocket.State);
                    _listener.OnStatusChanged(StatusCode.FailedToSend, message);
                }

                return false;
            }

            byte[] payload = null;

            switch (messageType)
            {
                case MessageType.RequestMessage:
                    payload = new RequestMessage(code, parameters, this, options.IsEncrypted).GetBytes();
                    break;

                case MessageType.EventMessage:
                    payload = new EventMessage(code, parameters, this, options.IsEncrypted).GetBytes();
                    break;

                default:
                    throw new ArgumentException("Invalid operation type");
            }

            CommandType type = options.QoS == QosType.ReliableSequenced ? CommandType.Reliable :
                options.QoS == QosType.UnreliableSequenced ? CommandType.Unreliable : CommandType.Reliable;

            return EnqueueCommand(type, payload, options.Channel, options.IsEncrypted);
        }

        private bool EnqueueCommand(CommandType commandType, byte[] payload, byte channel, bool encrypted)
        {
            if (channel >= ChannelCount)
            {
                if (_listener != null)
                {
                    string message = string.Format("Cannot send op: Selected channel ({0})>= channelCount ({1})", channel, ChannelCount);
                    _listener.OnStatusChanged(StatusCode.FailedToSend, message);
                }

                return false;
            }

            UdpChannel udpChannel = _channels[channel];

            if (payload != null && payload.Length > AvailableSpace) //fragmentation
            {
                ProcessFragmentation(udpChannel, payload, encrypted);
            }
            else //non-fragmentation
            {
                EnqueueOutgoingCommand(new OutgoingCommand(commandType, udpChannel.ChannelNumber, encrypted, payload));
            }

            return true;
        }

        private void ProcessFragmentation(UdpChannel udpChannel, byte[] payload, bool encrypted)
        {
            lock (udpChannel)
            {
                uint available = AvailableSpace;

                short fragmentCount = (short)((payload.Length + available - 1) / available);
                long startSequenceNumber = udpChannel.OutgoingReliableSequenceNumber + 1;
                short fragmentNumber = 0;

                for (long offset = 0; offset < payload.Length; offset += available)
                {
                    if (payload.Length - offset < available)
                    {
                        available = (uint)(payload.Length - offset);
                    }

                    byte[] array = new byte[available];

                    Buffer.BlockCopy(payload, (int)offset, array, 0, (int)available);

                    EnqueueOutgoingCommand(new OutgoingCommand(CommandType.Fragmented, udpChannel.ChannelNumber, encrypted, array)
                    {
                        FragmentNumber = fragmentNumber,
                        StartSequenceNumber = startSequenceNumber,
                        FragmentCount = fragmentCount,
                        FragmentOffset = offset,
                        TotalLength = payload.Length
                    });

                    fragmentNumber++;
                }
            }
        }

        internal void EnqueueOutgoingCommand(OutgoingCommand command)
        {
            UdpChannel udpChannel = _channels[command.Channel];
            lock (udpChannel)
            {
                SendQueueBase queue = command.IsReliable ? (SendQueueBase) udpChannel.ReliableSendQueue : udpChannel.UnreliableSendQueue;
                queue.EnqueueOutgoingCommand(command);
            }
        }

        private void EnqueueSentCommand(OutgoingCommand command, uint currentSendingTime)
        {
            UdpChannel channel = _channels[command.Channel];
            channel.ReliableSendQueue.EnqueueSentCommand(command, currentSendingTime, _roundTripTime.NewRoundTripTime(), (uint) DisconnectTimeout);

            if (_allowStatistics && command.SendAttempts > 1)
            {
                _statistics.ResendCommand();
            }
        }

        private bool EnqueueResendingCommands(uint currentTime)
        {
            foreach (var channel in _channels)
            {
                var resendingQueue = channel.ReliableSendQueue.FetchResendingCommands(currentTime, SentCountAllowance);

                while (resendingQueue.Count > 0)
                {
                    var command = resendingQueue.Dequeue();

                    if (command.IsTimeout(currentTime, SentCountAllowance))
                    {
                        Log(LogLevel.Info, "Timeout-disconnect [{0}:{1}]", RemoteEndPoint.Address, RemoteEndPoint.Port);
                        Disconnect(DisconnectReason.Timeout, false);
                        return false;
                    }

                    command.SentTime = currentTime;
                    EnqueueOutgoingCommand(command);
                }
            }

            return true;
        }
    }
}
