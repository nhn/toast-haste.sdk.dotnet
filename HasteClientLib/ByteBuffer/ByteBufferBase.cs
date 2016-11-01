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

namespace Haste.ByteBuffer
{
    public abstract class ByteBufferBase : IByteBuffer
    {
        internal const int DEFAULT_CAPACITY = 1500;
        protected int _writeIndex = 0;
        protected int _readIndex = 0;
        protected byte[] _data;
        
        public int ReadIndex { get { return _readIndex; } set { _readIndex = value; } }

        public int WriteIndex { get { return _writeIndex; } set { _writeIndex = value; } }

        public int Count { get { return _writeIndex; } }

        protected ByteBufferBase() : this(DEFAULT_CAPACITY)
        {
        }

        protected ByteBufferBase(int capacity)
        {
            _data = new byte[capacity];
        }

        #region Protected abstract methods
        protected abstract byte GetByte();
        protected abstract short GetShort();
        protected abstract int GetInt();
        protected abstract long GetLong();
        protected abstract float GetFloat();

        protected abstract void SetByte(byte data);
        protected abstract void SetShort(short data);
        protected abstract void SetInt(int data);
        protected abstract void SetLong(long data);
        protected abstract void SetFloat(float data);
        #endregion

        #region Priate helper methods
        private void EnsureReadIndex(int length)
        {
            if (_readIndex + length - 1 >= _data.Length)
                throw new IndexOutOfRangeException();
        }

        private void EnsureWriteIndex(int length)
        {
            if (_writeIndex + length - 1 >= _data.Length)
                throw new IndexOutOfRangeException();
        }
        #endregion

        #region Read methods
        public bool ReadBool()
        {
            EnsureReadIndex(sizeof(byte));
            byte result = GetByte();
            _readIndex += sizeof(byte);
            return result > 0;
        }

        public byte ReadByte()
        {
            EnsureReadIndex(sizeof(byte));
            byte result = GetByte();
            _readIndex += sizeof(byte);
            return result;
        }

        public void ReadBytes(byte[] output)
        {
            ReadBytes(output, 0, output.Length);
        }

        public void ReadBytes(byte[] output, int startIndex)
        {
            ReadBytes(output, startIndex, output.Length);
        }

        public void ReadBytes(byte[] output, int startIndex, int length)
        {
            EnsureReadIndex(length);
            for (int i = 0; i < length; i++)
            {
                output[startIndex + i] = _data[_readIndex + i];
            }
            _readIndex += length;
        }

        public int ReadInt()
        {
            EnsureReadIndex(sizeof(int));
            int result = GetInt();
            _readIndex += sizeof(int);
            return result;
        }

        public long ReadLong()
        {
            EnsureReadIndex(sizeof(long));
            long result = GetLong();
            _readIndex += sizeof(long);
            return result;
        }

        public short ReadShort()
        {
            EnsureReadIndex(sizeof(short));
            short result = GetShort();
            _readIndex += sizeof(short);
            return result;
        }

        public float ReadFloat()
        {
            EnsureReadIndex(sizeof(float));
            float result = GetFloat();
            _readIndex += sizeof(float);
            return result;
        }

        public double ReadDouble()
        {
            EnsureReadIndex(sizeof(long));
            long longTemp = GetLong();
            _readIndex += sizeof(long);
            return BitConverter.Int64BitsToDouble(longTemp);
        }
        #endregion Read methods

        #region Write methods
        public void WriteBool(bool data)
        {
            EnsureWriteIndex(sizeof(byte));
            SetByte((byte)(data ? 1 : 0));
            _writeIndex += sizeof(byte);
        }

        public void WriteByte(byte data)
        {
            EnsureWriteIndex(sizeof(byte));
            SetByte(data);
            _writeIndex += sizeof(byte);
        }

        public void WriteBytes(IByteBuffer buffer)
        {
            EnsureWriteIndex(buffer.Count);

            ByteBufferBase otherBuffer = buffer as ByteBufferBase;
            if (otherBuffer == null)
            {
                int oldReadIndex = buffer.ReadIndex;
                buffer.ReadIndex = 0;

                for (int i = 0; i < buffer.Count; i++)
                {
                    _data[_writeIndex + i] = buffer.ReadByte();
                }

                buffer.ReadIndex = oldReadIndex;
                _writeIndex += buffer.Count;
            }
            else
            {
                WriteBytes(otherBuffer._data, 0, buffer.Count);
            }
        }

        public void WriteBytes(byte[] data)
        {
            WriteBytes(data, 0, data.Length);
        }

        public void WriteBytes(byte[] data, int startIndex)
        {
            WriteBytes(data, startIndex, data.Length);
        }

        public void WriteBytes(byte[] data, int startIndex, int length)
        {
            EnsureWriteIndex(length);
            Buffer.BlockCopy(data, startIndex, _data, _writeIndex, length);
            _writeIndex += length;
        }

        public void WriteInt(int data)
        {
            EnsureWriteIndex(sizeof(int));
            SetInt(data);
            _writeIndex += sizeof(int);
        }

        public void WriteLong(long data)
        {
            EnsureWriteIndex(sizeof(long));
            SetLong(data);
            _writeIndex += sizeof(long);
        }

        public void WriteShort(short data)
        {
            EnsureWriteIndex(sizeof(short));
            SetShort(data);
            _writeIndex += sizeof(short);
        }

        public void WriteFloat(float data)
        {
            EnsureWriteIndex(sizeof(float));
            SetFloat(data);
            _writeIndex += sizeof(float);
        }

        public void WriteDouble(double data)
        {
            EnsureWriteIndex(sizeof(long));
            SetLong(BitConverter.DoubleToInt64Bits(data));
            _writeIndex += sizeof(long);
        }
        #endregion Write methods

        public byte[] ToArray()
        {
            byte[] newArray = new byte[_writeIndex];
            Buffer.BlockCopy(_data, 0, newArray, 0, _writeIndex);
            return newArray;
        }

        public void SeekReadIndex(int offset)
        {
            EnsureReadIndex(_readIndex + offset);
            _readIndex += offset;
        }

        public void SeekWriteIndex(int offset)
        {
            EnsureWriteIndex(_writeIndex + offset);
            _writeIndex += offset;
        }

        public string ToString(Encoding encoding)
        {
            return encoding.GetString(_data, _readIndex, _writeIndex - _readIndex);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(3 * _data.Length);
            for (int i = 0; i < _data.Length; i++)
            {
                builder.Append(_data[i].ToString("X2"));
                builder.Append(' ');
            }
            return base.ToString();
        }
    }
}
