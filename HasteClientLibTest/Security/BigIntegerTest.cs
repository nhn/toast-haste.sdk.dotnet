using System;
using System.Text;
using Haste.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HasteClientLibTest.Security
{
    [TestClass]
    public class BigIntegerTest
    {
        [TestMethod]
        public void ByteArrayToBigIntegerTest()
        {
            var bigInteger = new BigInteger("1234567891433", 10);
            var bytes = new byte[] { 1, 31, 113, 251, 9, 233 };
            var fromBytes = new BigInteger(bytes);
            Assert.AreEqual(bigInteger, fromBytes);
        }

        [TestMethod]
        public void ByteArrayToBigInteger2Test()
        {
            var bigInteger = new BigInteger("3234567891283", 10);
            var bytes = new byte[] { 2, 241, 27, 69, 41, 83 };
            var fromBytes = new BigInteger(bytes);
            Assert.AreEqual(bigInteger, fromBytes);

            var fromBytes2 = new BigInteger(new byte[] { 0, 157, 136, 54, 42, 61, 69, 136, 16, 174, 1, 39, 203, 249, 3, 108, 16, 143, 179, 2, 195, 29, 159, 117, 147, 41, 156, 57, 13, 236, 0, 44, 173, 67, 74, 44, 127, 141, 249, 255, 242, 18, 82, 251, 154, 189, 4, 94, 66, 55, 83, 5, 198, 173, 239, 116, 43, 113, 236, 65, 91, 54, 126, 222, 182, 27, 180, 219, 249, 152, 16, 94, 77, 83, 119, 211, 175, 132, 193, 211, 132, 229, 176, 84, 188, 193, 208, 25, 149, 125, 85, 213, 1, 106, 29, 203, 41 });
            Console.WriteLine(fromBytes2.ToString());
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ConvertBigIntegerTest()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1");
            builder.Append("29024E088A67CC74020BBEA63B139B22514A08798E3404DD");
            builder.Append("EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245");
            builder.Append("E485B576625E7EC6F44C42E9A63A3620FFFFFFFFFFFFFFFF");

            var num = new BigInteger(builder.ToString(), 16);
            Assert.AreEqual(97, num.ToByteArray().Length);
        }
    }
}
