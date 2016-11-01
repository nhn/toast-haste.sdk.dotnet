using System;
using System.Text;
using Haste.Security;

namespace HasteClientLibTest.Security
{
    internal class TestUtil
    {
        /// <summary>
        /// <para>These numbers are zero-index is non-zero.</para>
        /// <para>Item1 : BigInteger of Prime number</para>
        /// <para>Item2 : Byte array of Prime number</para>
        /// <para>Item3 : Byte array of Public key of Prime Number</para>
        /// </summary>
        internal static Tuple<BigInteger, byte[], byte[]>[] PrimeSet = {
            new Tuple<BigInteger, byte[], byte[]>(
                new BigInteger("1234567891433", 10), 
                new byte[]{1,31,113,251,9,233},
                new byte[]{0,165,54,92,171,75,137,79,245,156,163,152,160,69,245,248,253,5,48,164,222,167,195,34,206,150,77,22,255,170,9,100,218,206,206,172,96,54,47,246,234,106,249,209,34,148,86,26,235,167,201,126,39,226,103,133,231,177,171,113,90,86,70,189,135,152,106,1,211,26,165,140,16,57,138,131,104,107,114,236,203,247,116,110,26,41,108,198,218,150,231,126,71,191,154,243,10}),
            new Tuple<BigInteger, byte[], byte[]>(
                new BigInteger("3234567891283", 10), 
                new byte[]{2,241,27,69,41,83},
                new byte[]{0,157,136,54,42,61,69,136,16,174,1,39,203,249,3,108,16,143,179,2,195,29,159,117,147,41,156,57,13,236,0,44,173,67,74,44,127,141,249,255,242,18,82,251,154,189,4,94,66,55,83,5,198,173,239,116,43,113,236,65,91,54,126,222,182,27,180,219,249,152,16,94,77,83,119,211,175,132,193,211,132,229,176,84,188,193,208,25,149,125,85,213,1,106,29,203,41})
        };

        internal static string ToString(BigInteger num, bool isHex)
        {
            var bs = num.ToByteArray();
            return ToString(bs, isHex);
        }

        internal static string ToString(BigInteger num)
        {
            var bs = num.ToByteArray();
            return ToString(bs, false);
        }

        internal static string ToString(byte[] bs)
        {
            return ToString(bs, false);
        }

        internal static string ToString(byte[] bs, bool isHex)
        {
            var builder = new StringBuilder("[");
            for (int i = 0; i < bs.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }
                if (isHex)
                {
                    string s = bs[i].ToString("X");
                    builder.Append("0x");
                    builder.Append(s);
                }
                else
                {
                    builder.Append(bs[i]);
                }
            }
            return builder.Append("]").ToString();
        }
    }
}
