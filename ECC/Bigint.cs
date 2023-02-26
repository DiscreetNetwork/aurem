using System.Numerics;
using System.Runtime.InteropServices;

namespace Aurem.ECC
{
    /// <summary>
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BigInt
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Words;

        public static BigInteger Ensure256bits(BigInteger n) {
            byte[] bytes = n.ToByteArray();
            if (bytes.Length < 32)
                return n;
            byte[] _bytes = new byte[32];
            for (int c = 0; c < 32; c++) {
                _bytes[c] = bytes[c];
            }
            return new BigInteger(_bytes);
        }

        public static BigInteger ToBigInteger(BigInt n)
        {
            byte[] bytes = new byte[32];
            BigInteger x = new BigInteger(bytes);
            for (int i = 7; i >= 0; i--) {
                x <<= 64;
                x |= n.Words[i];
                x = Ensure256bits(x);
            }
            return x;
        }

        public static BigInteger ToBigInteger(ulong[] words)
        {
            byte[] bytes = new byte[32];
            BigInteger x = new BigInteger(bytes);
            for (int i = 7; i >= 0; i--) {
                x <<= 64;
                x |= words[i];
                x = Ensure256bits(x);
            }
            return x;
        }
    }
}
