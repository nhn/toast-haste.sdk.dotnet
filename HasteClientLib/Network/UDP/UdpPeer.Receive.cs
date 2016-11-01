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
using System.Text;
using System.Threading;
using Haste.ByteBuffer;
using Haste.Data;
using Haste.Network.Queues;

namespace Haste.Network
{
    internal partial class UdpPeer
    {
        /// <summary>
        /// Run queued actions that should call in a main thread.
        /// </summary>
        public void RunQueuedActions()
        {
            lock (_renderingLoopLock)
            {
                _toPassQueue = Interlocked.Exchange(ref _renderingLoopDispatchQueue, _toPassQueue);
                _renderingLoopDispatchQueue.Clear();
            }

            foreach (var action in _toPassQueue)
            {
                action();
            }
        }

        /// <summary>
        /// Read incoming commands. This method must be called in a main thread.
        /// If an application was developed by Unity3D, this method must be called in a script thread.
        /// </summary>
        public IEnumerable<IByteBuffer> ReadMessages()
        {
            if (_udpSocket.State != SocketState.Connected)
                yield return null;
            
            IncomingCommand command = null;
            List<IncomingCommand> commands = new List<IncomingCommand>();

            for (int i = 0; i < _channels.Count; i++)
            {
                // To prevent to receive a new command in a receive thread when read commands.
                // If a receive thread receive a new command when read commands, need to ignore a new command that receive when read commands.
                // If don't ignore a new command when read commands, continue processing a newer command.
                int unreliableCommandCount = _channels[i].UnreliableReceiveQueue.Count;
                int reliableCommandCount = _channels[i].ReliableReceiveQueue.Count;

                for (int j = 0; j < unreliableCommandCount; j++)
                {
                    if (_channels[i].UnreliableReceiveQueue.TryProcessCommand(out command))
                        commands.Add(command);
                }

                for (int j = 0; j < reliableCommandCount; j++)
                {
                    if (_channels[i].ReliableReceiveQueue.TryProcessCommand(out command))
                        commands.Add(command);
                }
            }

            foreach (var item in commands)
            {
                IByteBuffer payload = FetchPayload(item);
                if (payload == null)
                {
                    Disconnect(DisconnectReason.InvalidConnection, true);
                }
                yield return payload;
            }
        }

        /// <summary>
        /// Process a received command.
        /// (A ack command is processed by a receive thread, other commands is dispatched in a rendering thread.)
        /// </summary>
        private void ExecuteReceiveCommand(IncomingCommand command)
        {
            if (_udpSocket.State != SocketState.Connected)
                return;

            switch (command.Type)
            {
                case CommandType.Acknowledge:
                    // Already process a acknowledge command in a receive thread.
                    return;
                case CommandType.Disconnect:
                    byte[] payload = command.GetPayload();
                    int offset = 0;

                    DisconnectReason disconnectType = (DisconnectReason)ByteRead.GetInt(payload, ref offset);

                    var detailMessage = ByteRead.GetString<int>(payload, ref offset, Encoding.UTF8);
                    Log(LogLevel.Error, "Disconnect this client[{0}] : {1}", disconnectType, detailMessage);

                    Disconnect(disconnectType, false);

                    return;
                case CommandType.Reliable:
                case CommandType.Unreliable:
                    EnqueueIncomingCommand(command);
                    return;
                case CommandType.Fragmented:
                    if (command.FragmentNumber > command.FragmentCount ||
                        command.FragmentOffset >= command.TotalLength ||
                        command.FragmentOffset + command.GetPayload().Length > command.TotalLength)
                    {
                        Log(LogLevel.Error, "Received fragment has bad size: {0}", command);
                        return;
                    }

                    if (EnqueueIncomingCommand(command))
                    {
                        UdpChannel udpChannel = _channels[command.Channel];

                        ReliableReceiveQueue reliableReceiveQueue = udpChannel.ReliableReceiveQueue as ReliableReceiveQueue;
                        if (reliableReceiveQueue != null)
                            reliableReceiveQueue.ReceiveFragmentCommand(command);
                    }
                    return;
                default:
                    Log(LogLevel.Error, "Unknown command received {0}", command.Type);
                    return;
            }
        }

        /// <summary>
        /// Adds a command to the end of the incoming queue.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private bool EnqueueIncomingCommand(IncomingCommand command)
        {
            UdpChannel channel;
            if (!_channels.TryGetValue(command.Channel, out channel))
            {
                Log(LogLevel.Error, "Received a command for non-existing channel: {0}", command.Channel);
                return false;
            }

            ReceiveQueueBase receiveQueue = command.IsReliable ? channel.ReliableReceiveQueue : channel.UnreliableReceiveQueue;
            return receiveQueue.EnqueueIncomingCommand(command);
        }
    }
}
