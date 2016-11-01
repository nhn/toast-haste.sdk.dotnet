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
using Haste.ByteBuffer;

namespace Haste.Data
{
    /// <summary>
    /// Write byte array, endian is referenced from <seealso cref="DataSerializer.Endian"/>
    /// </summary>
    public static class ByteWrite
    {
        private static Endian _endian;
        static ByteWrite()
        {
            _endian = DataSerializer.Endian;
        }

        public static void SetByte(byte[] dst, ref int startIndex, byte value)
        {
            dst[startIndex] = value;
            startIndex++;
        }

        public static void SetShort(byte[] dst, ref int startIndex, short value)
        {
            byte[] src = BitConverter.GetBytes(value);

            if (ByteBufferUtil.IsReveresed(_endian))
            {
                Array.Reverse(src);
            }
            ByteBufferUtil.Copy(src, 0, dst, startIndex, src.Length);

            startIndex += sizeof(short);
        }

        public static void SetInt(byte[] dst, ref int startIndex, int value)
        {
            byte[] src = BitConverter.GetBytes(value);

            if (ByteBufferUtil.IsReveresed(_endian))
            {
                Array.Reverse(src);
            }
            ByteBufferUtil.Copy(src, 0, dst, startIndex, src.Length);

            startIndex += sizeof(int);
        }

        public static void SetLong(byte[] dst, ref int startIndex, long value)
        {
            byte[] src = BitConverter.GetBytes(value);

            if (ByteBufferUtil.IsReveresed(_endian))
            {
                Array.Reverse(src);
            }
            ByteBufferUtil.Copy(src, 0, dst, startIndex, src.Length);

            startIndex += sizeof(long);
        }

        public static void SetFloat(byte[] dst, ref int startIndex, float value)
        {
            byte[] src = BitConverter.GetBytes(value);

            if (ByteBufferUtil.IsReveresed(_endian))
            {
                Array.Reverse(src);
            }
            ByteBufferUtil.Copy(src, 0, dst, startIndex, src.Length);

            startIndex += sizeof(float);
        }
    }
}
