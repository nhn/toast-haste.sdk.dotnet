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

using System.Text;

namespace Haste.ByteBuffer
{
    public interface IByteBuffer
    {
        int ReadIndex { get; set; }
        int WriteIndex { get; set; }
        int Count { get; }
        
        bool ReadBool();
        byte ReadByte();
        short ReadShort();
        int ReadInt();
        long ReadLong();
        float ReadFloat();
        double ReadDouble();
        void ReadBytes(byte[] output);
        void ReadBytes(byte[] output, int startIndex);
        void ReadBytes(byte[] output, int startIndex, int length);

        void WriteBool(bool data);
        void WriteByte(byte data);
        void WriteShort(short data);
        void WriteInt(int data);
        void WriteLong(long data);
        void WriteFloat(float data);
        void WriteDouble(double data);
        void WriteBytes(IByteBuffer buffer);
        void WriteBytes(byte[] data);
        void WriteBytes(byte[] data, int startIndex);
        void WriteBytes(byte[] data, int startIndex, int length);

        void SeekReadIndex(int offset);
        void SeekWriteIndex(int offset);

        byte[] ToArray();
        string ToString(Encoding encoding);
    }
}