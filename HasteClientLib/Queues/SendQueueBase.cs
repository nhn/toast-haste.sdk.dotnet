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
    internal abstract class SendQueueBase
    {
        private UdpChannel _channel;
        private Queue<OutgoingCommand> _commands;

        protected UdpChannel Channel { get { return _channel; } }
        protected Queue<OutgoingCommand> Commands { get { return _commands; } }
        
        public int RemainingCommandCount { get { return _commands.Count; } }

        protected SendQueueBase(UdpChannel channel, int initialCapacity)
        {
            _channel = channel;
            _commands = new Queue<OutgoingCommand>(initialCapacity);
        }

        public void ClearCommands()
        {
            _commands.Clear();
        }

        public List<OutgoingCommand> DequeueSendableCommands(int bufferOffset, int bufferSize)
        {
            var result = new List<OutgoingCommand>();
            int index = bufferOffset;

            while (_commands.Count > 0)
            {
                OutgoingCommand command = _commands.Peek();

                if (command == null)
                    return result;

                //preventing buffer overflow, pendding command
                if (index + command.Size > bufferSize)
                    return result;

                index += command.Size;

                result.Add(_commands.Dequeue());
            }
            return result;
        }
        
        public abstract bool EnqueueOutgoingCommand(OutgoingCommand command);
    }
}