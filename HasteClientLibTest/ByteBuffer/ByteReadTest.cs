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
using Haste.ByteBuffer;
using Haste.Data;
using Haste.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HasteClientLibTest.ByteBuffer
{
    [TestClass]
    public class ByteReadTest
    {
        [TestMethod]
        public void ReadDisconnectPayloadTest()
        {
            var buffer = ByteBufferAllocator.NewBuffer(DataSerializer.Endian);

            var detailMessage = "Invalid CommandType";
            var srcDetail = Encoding.UTF8.GetBytes(detailMessage);
            buffer.WriteInt((int) DisconnectReason.InvalidDataFormat);
            buffer.WriteInt(srcDetail.Length);
            buffer.WriteBytes(srcDetail);

            byte[] payload = buffer.ToArray();
            int offset = 0;

            DisconnectReason disconnectType = (DisconnectReason) ByteRead.GetInt(payload, ref offset);

            int detailLength = ByteRead.GetInt(payload, ref offset);
            byte[] dstDetail = new byte[detailLength];
            ByteRead.ReadBytes(payload, dstDetail, offset, 0, detailLength);

            Assert.AreEqual(DisconnectReason.InvalidDataFormat, disconnectType);
            Assert.AreEqual(srcDetail.Length, detailLength);
            Assert.AreEqual(detailMessage, Encoding.UTF8.GetString(dstDetail));
        }

        [TestMethod]
        public void ReadStringTest()
        {
            var buffer = ByteBufferAllocator.NewBuffer(DataSerializer.Endian);

            var detailMessage = "Hello,World!!!";
            var detailBytes = Encoding.UTF8.GetBytes(detailMessage);
            buffer.WriteInt(detailBytes.Length);
            buffer.WriteBytes(detailBytes);

            int offset = 0;
            var result = ByteRead.GetString<int>(buffer.ToArray(), ref offset, Encoding.UTF8);
            Assert.AreEqual(detailMessage, result);
            Assert.AreEqual(sizeof(int) + detailMessage.Length, offset);
        }
    }
}