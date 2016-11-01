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
using System.Text;
using Haste.ByteBuffer;

namespace Haste.Data
{
    /// <summary>
    /// Read byte array, endian is referenced from <seealso cref="DataSerializer.Endian"/>
    /// </summary>
    public static class ByteRead
    {
        private static Endian _endian;
        static ByteRead()
        {
            _endian = DataSerializer.Endian;
        }

        public static byte GetByte(byte[] src, int startIndex)
        {
            return src[startIndex];
        }

        public static float GetFloat(byte[] src, int startIndex)
        {
            float result = BitConverter.ToSingle(src, startIndex);
            if (ByteBufferUtil.IsReveresed(_endian))
            {
                return result.Reverse();
            }
            return result;
        }

        public static int GetInt(byte[] src, int startIndex)
        {
            int result = BitConverter.ToInt32(src, startIndex);
            if (ByteBufferUtil.IsReveresed(_endian))
            {
                return result.Reverse();
            }
            return result;
        }

        public static long GetLong(byte[] src, int startIndex)
        {
            long result = BitConverter.ToInt64(src, startIndex);
            if (ByteBufferUtil.IsReveresed(_endian))
            {
                return result.Reverse();
            }
            return result;
        }

        public static short GetShort(byte[] src, int startIndex)
        {
            short result = BitConverter.ToInt16(src, startIndex);
            if (ByteBufferUtil.IsReveresed(_endian))
            {
                return result.Reverse();
            }
            return result;
        }

        public static void ReadBytes(byte[] src, byte[] dst, int srcStartIndex, int dstStartIndex, int length)
        {
            for (int i = 0; i < length; i++)
            {
                dst[dstStartIndex + i] = src[srcStartIndex + i];
            }
        }
        
        public static string GetString<LengthType>(byte[] src, ref int startIndex, Encoding encoding)
        {
            int length = 0;
            if (typeof(LengthType) == typeof(int))
                length = GetInt(src, ref startIndex);
            else if (typeof(LengthType) == typeof(short))
                length = GetShort(src, ref startIndex);
            else if (typeof(LengthType) == typeof(byte))
                length = GetByte(src, ref startIndex);

            byte[] bytes = new byte[length];
            ReadBytes(src, bytes, startIndex, 0, length);
            startIndex += length;
            return encoding.GetString(bytes);
        }

        public static byte GetByte(byte[] src, ref int offset)
        {
            byte result = GetByte(src, offset);
            offset += sizeof(byte);
            return result;
        }

        public static float GetFloat(byte[] src, ref int offset)
        {
            float result = GetFloat(src, offset);
            offset += sizeof(float);
            return result;
        }

        public static int GetInt(byte[] src, ref int offset)
        {
            int result = GetInt(src, offset);
            offset += sizeof(int);
            return result;
        }

        public static long GetLong(byte[] src, ref int offset)
        {
            long result = GetLong(src, offset);
            offset += sizeof(long);
            return result;
        }

        public static short GetShort(byte[] src, ref int offset)
        {
            short result = GetShort(src, offset);
            offset += sizeof(short);
            return result;
        }
    }
}
