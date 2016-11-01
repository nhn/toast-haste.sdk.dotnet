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

using System.Collections;
using System.Collections.Generic;

namespace Haste.Network
{
    internal class UdpChannelList : IEnumerable<UdpChannel>
    {
        private const int DefaultInitialCapacity = 1024;

        private UdpChannel[] _channels;

        public UdpChannelList(ConnectionConfig config, int channelCount)
            : this(config, channelCount, DefaultInitialCapacity)
        {
        }

        public UdpChannelList(ConnectionConfig config, int channelCount, int initialCapacity)
        {
            _channels = new UdpChannel[channelCount];
            for (byte i = 0; i < channelCount; i++)
            {
                _channels[i] = new UdpChannel(config, i, initialCapacity);
            }
        }
        
        public UdpChannel this[int channel] { get { return _channels[channel]; } }

        public int Count { get { return _channels.Length; } }

        public bool TryGetValue(int channel, out UdpChannel outChannel)
        {
            if (channel >= _channels.Length || channel < 0)
            {
                outChannel = null;
                return false;
            }

            outChannel = _channels[channel];
            return outChannel != null;
        }

        public void ClearAll()
        {
            for (int i = 0; i < _channels.Length; i++)
            {
                _channels[i].ClearAll();
            }
        }

        public IEnumerator<UdpChannel> GetEnumerator()
        {
            IEnumerable<UdpChannel> enumerable = _channels;
            return enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
