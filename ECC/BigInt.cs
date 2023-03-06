using System.Numerics;
using System.Runtime.InteropServices;

namespace Aurem.ECC
{
    public struct BigInt8
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Words;
    }

    /// <summary>
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BigInt
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ulong[] Words;

        private static BigInteger Order = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");

        public BigInt()
        {
            Words = new ulong[4];
        }

        public BigInt(ulong n)
        {
            Words = new ulong[4];
            Words[0] = n;
        }

        public BigInt(BigInteger n)
        {
            Words = new ulong[4];
            for (int i = 0; i < 4; i++)
            {
                Words[i] = (ulong)(n & ulong.MaxValue);
                n >>= 64;
            }
        }

        public void Print(string msg)
        {
            Console.WriteLine($"{msg} {ToBigInteger(this)}");
        }

        // TODO Implement addition operation handling the ulong values directly.
        public static BigInt Add(BigInt left, BigInt right)
        {
            return new BigInt(BigInteger.Add(ToBigInteger(left), ToBigInteger(right)) % Order);

        }

        public static BigInt Multiply(BigInt left, BigInt right)
        {
            return new BigInt(BigInteger.Multiply(ToBigInteger(left), ToBigInteger(right)) % Order);
        }

        public static BigInt Power(BigInt left, BigInt right)
        {
            return new BigInt(BigInteger.ModPow(ToBigInteger(left), ToBigInteger(right), Order));
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
            for (int i = 3; i >= 0; i--) {
                x <<= 64;
                x |= n.Words[i];
                x = Ensure256bits(x);
            }
            return x;
        }

        public static BigInteger ToBigInteger(ulong[] words)
        {
            BigInteger x = 0;
            for (int i = 3; i >= 0; i--) {
                x <<= 64;
                x |= words[i];
                x = Ensure256bits(x);
            }
            return x;
        }
    }
}
