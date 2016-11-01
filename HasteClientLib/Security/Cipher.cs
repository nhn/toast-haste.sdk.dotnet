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
using System.Threading;

namespace Haste.Security
{
    public class Cipher
    {
        private const int PrivateKeyBitLength = 160;

        private Rijndael _aes;

        private BigInteger _secretKey;

        private BigInteger _privateKey;
        private BigInteger _publicKey;

        public bool IsInitialized
        {
            get { return _aes != null; }
        }

        public BigInteger PublicKey { get { return _publicKey; } }

        public byte[] Decrypt(byte[] data)
        {
            return Decrypt(data, 0, data.Length);
        }

        public byte[] Decrypt(byte[] data, int offset, int count)
        {
            using (ICryptoTransform cryptoTransform = _aes.CreateDecryptor())
            {
                return cryptoTransform.TransformFinalBlock(data, offset, count);
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            return Encrypt(data, 0, data.Length);
        }

        public byte[] Encrypt(byte[] data, int offset, int count)
        {
            byte[] result;
            using (ICryptoTransform cryptoTransform = _aes.CreateEncryptor())
            {
                result = cryptoTransform.TransformFinalBlock(data, offset, count);
            }
            return result;
        }

        public void EstablishKeyExchange(BigInteger serverKey)
        {
            BigInteger secretKey = DiffieHellman.GenerateSecretKey(serverKey, _privateKey);
            Interlocked.Exchange(ref _secretKey, secretKey);

            byte[] key = _secretKey.ToByteArray();

            Interlocked.Exchange(ref _aes, Rijndael.Create());

            _aes.Key = Hash.SHA256.Hash(key);

            _aes.IV = new byte[16];
            _aes.Padding = PaddingMode.PKCS7;
        }

        public void GenerateLocalKeys()
        {
            _privateKey = DiffieHellman.GeneratePrivateKey(PrivateKeyBitLength);
            _publicKey = DiffieHellman.GeneratePublicKey(_privateKey);
        }
    }
}