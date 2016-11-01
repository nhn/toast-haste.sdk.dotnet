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
    public class BigEndianTest
    {
        [TestMethod]
        public void GetTest()
        {
            var bigEndianBuffer = ByteBufferAllocator.NewBuffer(Endian.BigEndian);
            bigEndianBuffer.WriteInt(10);

            var littleEndianBuffer = ByteBufferAllocator.NewReadOnlyBuffer(Endian.LittleEndian, bigEndianBuffer.ToArray());
            Assert.AreEqual(10.Reverse(), littleEndianBuffer.ReadInt());
        }
    }
}