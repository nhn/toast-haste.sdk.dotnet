﻿/*
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

using Haste.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HasteClientLibTest.Network
{
    [TestClass]
    public class RoundTripTimeTest
    {
        [TestMethod]
        public void BackOffMultipleTest()
        {
            uint[] expectedBackOffMultiple = { 1, 1, 2, 4, 8, 16 };
            for (uint i = 0; i < 10; i++)
            {
                var backOff = RoundTripTime.GetBackOffMultiple(i);
                uint index = (i >= expectedBackOffMultiple.Length) ? (uint)(expectedBackOffMultiple.Length - 1) : i;
                Assert.AreEqual(expectedBackOffMultiple[index], backOff);
            }
        }
    }
}
