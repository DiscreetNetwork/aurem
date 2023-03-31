using System.Numerics;
using Aurem.ECC;
using Aurem.ECC.Native;

/// <summary>
/// Implementation of components needed to achieve Common Randomness for Common
/// Vote among the nodes in Aurem.
/// </summary>
namespace Aurem.Randomness
{
    /// <summary>
    /// </summary>
    public class ThresholdSignature
    {
        private int threshold;
        private int nParties;

        public ThresholdSignature(int _threshold, int _nParties)
        {
            threshold = _threshold;
            nParties = _nParties;
         }

        /// <summary>
        /// GenerateKeys
        /// </summary>
        public (VerificationKey vkey, List<SecretKey> skeys) GenerateKeys()
        {
            AltBn128G2 G2 = Native.Instance.G2();
            // Generating a set of coefficients.
            List<BigInt> ZRs = new();
            for (int c = 0; c < threshold; c++) {
                ZRs.Add(Native.Instance.RandomCoefficient());
            }
            BigInt secret = ZRs[threshold-1];

            // Generating secret keys.
            List<BigInt> sks = new();
            for (ulong c = 0; c < (ulong)nParties; c++)
                sks.Add(EvaluatePolynomial(ZRs, new BigInt(c)));

            // Generating verification keys.
            AltBn128G2 vk = Native.Instance.ScalarMulG2(secret);
            List<AltBn128G2> vks = new();

            foreach (BigInt sk in sks)
                vks.Add(Native.Instance.ScalarMulG2(sk));

            VerificationKey verificationKey = new(threshold, vk, vks);
            List<SecretKey> secretKeys = new();

            foreach(BigInt sk in sks)
                secretKeys.Add(new SecretKey(sk));

            return (verificationKey, secretKeys);
        }

        private BigInt EvaluatePolynomial(List<BigInt> coefficients, BigInt x)
        {
            BigInt result = new BigInt(0);
            for (int c = 0; c < coefficients.Count; c++) {
                // x * y + coef, and we reduce.
                result = BigInt.Add(result, BigInt.Add(coefficients[c], BigInt.Multiply(x, result)));
            }
            return result;
        }
    }

    /// <summary>
    /// </summary>
    public class VerificationKey
    {
        private int threshold;
        private AltBn128G2 vk;
        private List<AltBn128G2> vks;
        private AltBn128G2 G2;
        private BigInteger Order = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");

        public VerificationKey(int _threshold, AltBn128G2 _vk, List<AltBn128G2> _vks) {
            threshold = _threshold;
            vk = _vk;
            vks = _vks;
            G2 = Native.Instance.G2();
        }

        public static BigInteger ModDivision(BigInteger a, BigInteger b, BigInteger r)
        {
            BigInteger gcd, x, y;
            (gcd, x, y) = ExtendedEuclideanAlgorithm(b, r);
            if (gcd != 1)
            {
                throw new ArgumentException("b does not have a modular inverse modulo r");
            }
            BigInteger bInv = (x % r + r) % r; // Ensure bInv is positive
            return (a * bInv) % r;
        }

        public static (BigInteger, BigInteger, BigInteger) ExtendedEuclideanAlgorithm(BigInteger a, BigInteger b)
        {
            BigInteger x = 0, y = 1, lastx = 1, lasty = 0, temp;
            while (b != 0)
            {
                BigInteger quotient = a / b;
                BigInteger remainder = a % b;

                a = b;
                b = remainder;

                temp = x;
                x = lastx - quotient * x;
                lastx = temp;

                temp = y;
                y = lasty - quotient * y;
                lasty = temp;
            }
            return (a, lastx, lasty);
        }

        /// <summary>
        /// </summary>
        private BigInt Lagrange(List<long> shareIdxs, long ownerIdx)
        {
            BigInteger num = 1, den = 1;
            foreach (int idx in shareIdxs) {
                if (idx == ownerIdx) continue;
                num *= (0 - idx - 1);
                den *= (ownerIdx - idx);
            }
            if (num < 0)
                num = Order + num;
            if (den < 0)
                den = Order + den;

            return new BigInt(ModDivision(num, den, Order));
        }

        /// <summary>
        /// </summary>
        public AltBn128G1 CombineShares(Dictionary<long, AltBn128G1> shares)
        // public BigInt CombineShares(Dictionary<int, BigInt> shares)
        {
            List<long> idxs = shares.Keys.ToList();
            idxs.Sort();
            AltBn128G1 res = Native.Instance.ScalarPointMulG1(shares[idxs[0]], Lagrange(idxs, idxs[0]));

            for (int i = 1; i < idxs.Count; i++) {
                res = Native.Instance.AddG1(
                    res,
                    Native.Instance.ScalarPointMulG1(shares[idxs[i]], Lagrange(idxs , idxs[i])));
            }
            return res;
        }

        /// <summary>
        /// Verifies if the share of a message hash was generated by the party
        /// who owns the verification key at vkIdx.
        /// </summary>
        public bool VerifyShare(AltBn128G1 share, int vkIdx, BigInt msg)
        {
            return Native.Instance.PairsEqual(Native.Instance.ScalarMulG1(msg), vks[vkIdx], share, G2);
        }

        /// <summary>
        /// </summary>
        public bool VerifySignature(AltBn128G1 signature, BigInt msg)
        {
            return Native.Instance.PairsEqual(Native.Instance.ScalarMulG1(msg), vk, signature, G2);
        }
    }

    /// <summary>
    /// </summary>
    public class SecretKey
    {
        private BigInt sk;

        public SecretKey(BigInt _sk)
        {
            sk = _sk;
        }

        /// <summary>
        /// </summary>
        public AltBn128G1 GenerateShare(BigInt msg)
        {
            return Native.Instance.ScalarMulG1(BigInt.Multiply(msg, sk));
        }
    }
}
