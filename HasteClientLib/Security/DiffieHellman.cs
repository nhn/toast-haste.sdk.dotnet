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

namespace Haste.Security
{
    internal class DiffieHellman
    {
        private static readonly BigInteger _baseNumber = OakleyGroup1.Generator;
        private static readonly BigInteger _primeNumber = OakleyGroup1.PrimeNumber;
        private static readonly Random _random = new Random();

        public static BigInteger GenerateSecretKey(BigInteger remotePublicKey, BigInteger privateKey)
        {
            return remotePublicKey.ModPow(privateKey, _primeNumber);
        }

        public static BigInteger GeneratePublicKey(BigInteger privateKey)
        {
            return _baseNumber.ModPow(privateKey, _primeNumber);
        }

        public static BigInteger GeneratePrivateKey(int bitLength)
        {
            return BigInteger.ProbablePrime(bitLength, _random);
        }
    }
}
