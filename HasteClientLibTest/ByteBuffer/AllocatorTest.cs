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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HasteClientLibTest
{
    [TestClass]
    public class AllocatorTest
    {
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ReadonlyBufferTest()
        {
            var buffer = ByteBufferAllocator.NewReadOnlyBuffer(Endian.BigEndian, new byte[8]);
            buffer.WriteByte(0);
        }

        [TestMethod]
        public void WritableBufferTest()
        {
            var buffer = ByteBufferAllocator.NewBuffer(Endian.BigEndian, new byte[1] { 1 });
            buffer.WriteByte(2);

            Assert.AreEqual(1, buffer.ReadByte());
            Assert.AreEqual(2, buffer.ReadByte());
        }
    }
}
