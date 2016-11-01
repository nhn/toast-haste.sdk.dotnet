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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HasteClientLibTest
{
    [TestClass]
    public class ReadWriteTest
    {
        [TestMethod]
        public void LittleEndianTest()
        {
            EndianTest(Endian.LittleEndian);
        }

        [TestMethod]
        public void BigEndianTest()
        {
            EndianTest(Endian.BigEndian);
        }

        private void EndianTest(Endian endian)
        {
            var buffer = ByteBufferAllocator.NewBuffer(endian);
            buffer.WriteBool(true);
            buffer.WriteByte(byte.MaxValue);
            buffer.WriteShort(short.MaxValue);
            buffer.WriteInt(int.MaxValue);
            buffer.WriteLong(long.MaxValue);
            buffer.WriteFloat(float.MaxValue);
            buffer.WriteDouble(double.MaxValue);

            Assert.AreEqual(true, buffer.ReadBool());
            Assert.AreEqual(byte.MaxValue, buffer.ReadByte());
            Assert.AreEqual(short.MaxValue, buffer.ReadShort());
            Assert.AreEqual(int.MaxValue, buffer.ReadInt());
            Assert.AreEqual(long.MaxValue, buffer.ReadLong());
            Assert.AreEqual(float.MaxValue, buffer.ReadFloat());
            Assert.AreEqual(double.MaxValue, buffer.ReadDouble());
        }
    }
}
