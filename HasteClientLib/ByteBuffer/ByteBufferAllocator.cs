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

using Haste.ByteBuffer.EndianBuffer;
using System;

namespace Haste.ByteBuffer
{
    public static class ByteBufferAllocator
    {
        public static IByteBuffer NewBuffer(Endian endian)
        {
            return NewBuffer(endian, ByteBufferBase.DEFAULT_CAPACITY);
        }

        public static IByteBuffer NewBuffer(Endian endian, byte[] data)
        {
            int capacity = Math.Max(ByteBufferBase.DEFAULT_CAPACITY, (int)(data.Length * 1.5));
            IByteBuffer buffer = NewBuffer(endian, capacity);
            buffer.WriteBytes(data);
            return buffer;
        }

        public static IByteBuffer NewReadOnlyBuffer(Endian endian, byte[] data)
        {
            IByteBuffer buffer = NewBuffer(endian, data.Length);
            buffer.WriteBytes(data);
            return buffer;
        }

        public static IByteBuffer NewBuffer(Endian endian, int capacity)
        {
            if (ByteBufferUtil.IsReveresed(endian))
            {
                return new ReversedByteBuffer(capacity);
            }
            return new NormalByteBuffer(capacity);
        }
    }
}
