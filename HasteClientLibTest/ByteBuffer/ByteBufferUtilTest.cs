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
    public class ByteBufferUtilTest
    {
        [TestMethod]
        public void ReverseMethodsTest()
        {
            var length = 1000;
            var rnd = new Random();
            var i = 0;

            // Short test
            for (i = 0; i < length; i++)
            {
                short tc = (short)rnd.Next(short.MaxValue);
                var bs = BitConverter.GetBytes(tc);
                Array.Reverse(bs);
                Assert.AreEqual(BitConverter.ToInt16(bs, 0), tc.Reverse());
            }

            // Int test
            for (i = 0; i < length; i++)
            {
                int tc = rnd.Next(int.MaxValue);
                var bs = BitConverter.GetBytes(tc);
                Array.Reverse(bs);
                Assert.AreEqual(BitConverter.ToInt32(bs, 0), tc.Reverse());
            }

            // Long test
            for (i = 0; i < length; i++)
            {
                long tc = rnd.Next();
                var bs = BitConverter.GetBytes(tc);
                Array.Reverse(bs);
                Assert.AreEqual(BitConverter.ToInt64(bs, 0), tc.Reverse());
            }

            // Float test
            for (i = 0; i < length; i++)
            {
                float tc = (float)rnd.NextDouble();
                var bs = BitConverter.GetBytes(tc);
                Array.Reverse(bs);
                Assert.AreEqual(BitConverter.ToSingle(bs, 0), tc.Reverse());
            }

            // Double test
            for (i = 0; i < length; i++)
            {
                double tc = rnd.NextDouble();
                var bs = BitConverter.GetBytes(tc);
                Array.Reverse(bs);
                Assert.AreEqual(BitConverter.ToDouble(bs, 0), tc.Reverse());
            }
        }
    }
}
