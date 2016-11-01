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

using Haste.ByteBuffer;

namespace Haste.Data
{
    internal static partial class DataSerializer
    {
        private const int MaxLength = short.MaxValue;

        public static byte Version { get { return 0x02; } }

        public static Endian Endian { get { return Version >= 0x02 ? Endian.BigEndian : Endian.LittleEndian; } }

        public static DataObject ToDataObject(this byte[] binary)
        {
            IByteBuffer buffer = ByteBufferFactory.NewBuffer(binary);
            return buffer.ToDataObject();
        }

        public static DataObject ToDataObject(this IByteBuffer buffer)
        {
            return ReadDataObject(buffer);
        }
    }
}