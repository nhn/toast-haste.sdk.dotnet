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

namespace Haste.Security
{
    internal class OakleyGroup1
    {
        // Reference to https://tools.ietf.org/html/rfc2409#page-21
        public static readonly BigInteger Generator = BigInteger.ValueOf(2);
        private static readonly byte[] _oakley768 = {
            0x00, // the most significant byte is in the zeroth element.
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xC9,
            0x0F,
            0xDA,
            0xA2,
            0x21,
            0x68,
            0xC2,
            0x34,
            0xC4,
            0xC6,
            0x62,
            0x8B,
            0x80,
            0xDC,
            0x1C,
            0xD1,
            0x29,
            0x02,
            0x4E,
            0x08,
            0x8A,
            0x67,
            0xCC,
            0x74,
            0x02,
            0x0B,
            0xBE,
            0xA6,
            0x3B,
            0x13,
            0x9B,
            0x22,
            0x51,
            0x4A,
            0x08,
            0x79,
            0x8E,
            0x34,
            0x04,
            0xDD,
            0xEF,
            0x95,
            0x19,
            0xB3,
            0xCD,
            0x3A,
            0x43,
            0x1B,
            0x30,
            0x2B,
            0x0A,
            0x6D,
            0xF2,
            0x5F,
            0x14,
            0x37,
            0x4F,
            0xE1,
            0x35,
            0x6D,
            0x6D,
            0x51,
            0xC2,
            0x45,
            0xE4,
            0x85,
            0xB5,
            0x76,
            0x62,
            0x5E,
            0x7E,
            0xC6,
            0xF4,
            0x4C,
            0x42,
            0xE9,
            0xA6,
            0x3A,
            0x36,
            0x20,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF
    };
        public static BigInteger PrimeNumber = new BigInteger(_oakley768);
    }
}