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

        public BigInt()
        {
            Words = new ulong[8];
        }

        public BigInt(ulong n)
        {
            Words = new ulong[8];
            Words[0] = n;
        }

        public BigInt(BigInteger n)
        {
            byte[] bytes = n.ToByteArray();
            int length = Math.Min(bytes.Length, 32);
            Words = new ulong[8];

            for (int i = 0; i < length; i++) {
                int ulongIndex = i / 8;
                int byteIndex = i % 8;
                Words[ulongIndex] |= ((ulong)bytes[i] << (byteIndex * 8));
            }
        }

        public void Print()
        {
            Console.WriteLine(ToBigInteger(this));
        }

        // TODO Implement addition operation handling the ulong values directly.
        public static BigInt Add(BigInt left, BigInt right)
        {
            return new BigInt(BigInteger.Add(ToBigInteger(left), ToBigInteger(right)));
        }

        public static BigInt Multiply(BigInt left, BigInt right)
        {
            return new BigInt(BigInteger.Multiply(ToBigInteger(left), ToBigInteger(right)));
        }

        public static BigInteger Ensure256bits(BigInteger n)
        {
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
            BigInteger x = 0;
            for (int i = 7; i >= 0; i--) {
                x <<= 64;
                x |= n.Words[i];
                x = Ensure256bits(x);
            }
            return x;
        }

        public static BigInteger ToBigInteger(ulong[] words)
        {
            BigInteger x = 0;
            for (int i = 7; i >= 0; i--) {
                x <<= 64;
                x |= words[i];
                x = Ensure256bits(x);
            }
            return x;
        }
    }
}
