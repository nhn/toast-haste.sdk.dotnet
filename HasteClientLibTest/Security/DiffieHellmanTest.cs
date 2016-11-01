using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HasteClientLibTest.Security
{
    using Haste.Security;

    [TestClass]
    public class DiffieHellmanTest
    {
        private void DHTest(BigInteger clientPrivateKey, BigInteger clientPublicKey, BigInteger serverPrivateKey, BigInteger serverPublicKey)
        {
            StackFrame[] frames = new StackTrace().GetFrames();
            BigInteger serverSecretKey = DiffieHellman.GenerateSecretKey(clientPublicKey, serverPrivateKey);
            BigInteger clientSecretKey = serverPublicKey.ModPow(clientPrivateKey, OakleyGroup1.PrimeNumber);

            byte[] serverSecretkeyMD5 = Hash.MD5.Hash(serverSecretKey);
            byte[] clientSecretkeyMD5 = Hash.MD5.Hash(clientSecretKey);

            Assert.AreEqual(clientSecretKey, serverSecretKey);
            CollectionAssert.AreEqual(Hash.SHA256.Hash(clientSecretKey), Hash.SHA256.Hash(serverSecretKey));
            CollectionAssert.AreEqual(Hash.MD5.Hash(clientSecretKey), Hash.MD5.Hash(serverSecretKey));
            
            Console.WriteLine("============================================");
            Console.WriteLine(string.Format("[{0}]", frames[1].GetMethod().Name));
            Console.WriteLine("ClientPrivateKey : " + TestUtil.ToString(clientPrivateKey));
            Console.WriteLine("ClientPublicKey : " + TestUtil.ToString(clientPublicKey));
            Console.WriteLine("ServerPrivateKey : " + TestUtil.ToString(serverPrivateKey));
            Console.WriteLine("ServerPublicKey : " + TestUtil.ToString(serverPublicKey));
            Console.WriteLine("ServerSecretKey : " + TestUtil.ToString(serverSecretKey));
            Console.WriteLine("ClientSecretKey : " + TestUtil.ToString(clientSecretKey));
            Console.WriteLine("============================================");
            Console.WriteLine("ServerSecretKey(MD5) : " + TestUtil.ToString(serverSecretkeyMD5));
            Console.WriteLine("ClientSecretKey(MD5) : " + TestUtil.ToString(clientSecretkeyMD5));
            Console.WriteLine("============================================");
        }

        [TestMethod]
        public void SimpleDiffieHellmanTest()
        {
            BigInteger clientPrivateKey = TestUtil.PrimeSet[0].Item1;
            BigInteger clientPublicKey = OakleyGroup1.Generator.ModPow(clientPrivateKey, OakleyGroup1.PrimeNumber);

            BigInteger serverPrivateKey = TestUtil.PrimeSet[1].Item1;
            BigInteger serverPublicKey = DiffieHellman.GeneratePublicKey(serverPrivateKey);

            DHTest(clientPrivateKey, clientPublicKey, serverPrivateKey, serverPublicKey);
        }

        [TestMethod]
        public void GeneratedServerPublicKeyTest()
        {
            BigInteger serverPrivateKey = new BigInteger("3234567891283", 10);
            BigInteger serverPublicKey = new BigInteger(new byte[]
            {
                0, 157, 136, 54, 42, 61, 69, 136, 16, 174, 1, 39, 203, 249, 3, 108, 16, 143, 179, 2, 195, 29, 159, 117,
                147, 41, 156, 57, 13, 236, 0, 44, 173, 67, 74, 44, 127, 141, 249, 255, 242, 18, 82, 251, 154, 189, 4, 94,
                66, 55, 83, 5, 198, 173, 239, 116, 43, 113, 236, 65, 91, 54, 126, 222, 182, 27, 180, 219, 249, 152, 16,
                94, 77, 83, 119, 211, 175, 132, 193, 211, 132, 229, 176, 84, 188, 193, 208, 25, 149, 125, 85, 213, 1,
                106, 29, 203, 41
            });

            BigInteger clientPrivateKey = new BigInteger("1234567891433", 10);
            BigInteger clientPublicKey = DiffieHellman.GeneratePublicKey(clientPrivateKey);

            DHTest(clientPrivateKey, clientPublicKey, serverPrivateKey, serverPublicKey);
        }
    }
}