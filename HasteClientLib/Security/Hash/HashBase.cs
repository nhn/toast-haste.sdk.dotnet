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

using System.Security.Cryptography;

namespace Haste.Security
{
    internal abstract class HashBase
    {
        public byte[] Hash(BigInteger num)
        {
            return Hash(num.ToByteArray());
        }

        public byte[] Hash(byte[] src)
        {
            return CalculateHash(src);
        }

        protected abstract string Algorithm { get; }

        private byte[] CalculateHash(byte[] src)
        {
            using (var hash = HashAlgorithm.Create(Algorithm))
            {
                return hash.ComputeHash(src);
            }
        } 
    }
}
