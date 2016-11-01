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

namespace Haste.Messages
{
    internal sealed class InitialRequest
    {
        private byte[] _bytes = null;

        public InitialRequest(
            Version sdkVersion,
            Version clientVersion,
            byte[] customData)
        {
            var buffer = ByteBufferFactory.NewBuffer();
            buffer.WriteByte(DataSerializer.Version);
            buffer.WriteByte((byte)MessageType.InitialRequest);

            buffer.WriteShort((short)sdkVersion.Major);
            buffer.WriteShort((short)sdkVersion.Minor);
            buffer.WriteShort((short)sdkVersion.Build);

            buffer.WriteShort((short)clientVersion.Major);
            buffer.WriteShort((short)clientVersion.Minor);
            buffer.WriteShort((short)clientVersion.Build);

            int customDataLength = customData == null ? 0 : customData.Length;
            buffer.WriteShort((short)customDataLength);
            
            if (customDataLength > 200)
            {
                _bytes = null;
                return;
            }

            if (customData != null)
                buffer.WriteBytes(customData, 0, customDataLength);

            _bytes = buffer.ToArray();
        }

        public byte[] GetBytes()
        {
            return _bytes;
        }
    }
}