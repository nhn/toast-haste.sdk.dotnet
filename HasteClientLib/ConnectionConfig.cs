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

namespace Haste
{
    public class ConnectionConfig
    {
        public short ChannelCount { get; set; }
        public short MtuSize { get; set; }
        public int PingInterval { get; set; }
        public int DisconnectionTimeout { get; set; }
        public int MaxUnreliableCommands { get; set; }
        public bool IsCrcEnabled { get; set; }
        public int SentCountAllowance { get; set; }
        public bool AllowStatistics { get; set; }
        public PortRange BindPortRange { get; set; }
    }
}
