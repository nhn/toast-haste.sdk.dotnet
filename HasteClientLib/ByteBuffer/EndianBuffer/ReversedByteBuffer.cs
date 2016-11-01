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

namespace Haste.ByteBuffer.EndianBuffer
{
    internal class ReversedByteBuffer : ByteBufferBase
    {
        internal ReversedByteBuffer() : this(DEFAULT_CAPACITY) { }

        internal ReversedByteBuffer(int capacity) : base(capacity)
        {
        }
       
        protected override byte GetByte()
        {
            return _data[_readIndex];
        }

        protected override float GetFloat()
        {
            return BitConverter.ToSingle(_data, _readIndex).Reverse();
        }

        protected override int GetInt()
        {
            return BitConverter.ToInt32(_data, _readIndex).Reverse();
        }

        protected override long GetLong()
        {
            return BitConverter.ToInt64(_data, _readIndex).Reverse();
        }

        protected override short GetShort()
        {
            return BitConverter.ToInt16(_data, _readIndex).Reverse();
        }

        protected override void SetByte(byte data)
        {
            _data[_writeIndex] = data;
        }

        protected override void SetFloat(float data)
        {
            var bytes = BitConverter.GetBytes(data.Reverse());
            ByteBufferUtil.Copy(bytes, 0, _data, _writeIndex, sizeof(float));
        }

        protected override void SetInt(int data)
        {
            var bytes = BitConverter.GetBytes(data.Reverse());
            ByteBufferUtil.Copy(bytes, 0, _data, _writeIndex, sizeof(int));
        }

        protected override void SetLong(long data)
        {
            var bytes = BitConverter.GetBytes(data.Reverse());
            ByteBufferUtil.Copy(bytes, 0, _data, _writeIndex, sizeof(long));
        }

        protected override void SetShort(short data)
        {
            var bytes = BitConverter.GetBytes(data.Reverse());
            ByteBufferUtil.Copy(bytes, 0, _data, _writeIndex, sizeof(short));
        }
    }
}
