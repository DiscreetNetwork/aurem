using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using System.Text;
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
    class BN254
    {
        public BigInteger p;
        public BigInteger a;
        public BigInteger b;
        public BigInteger q;
        public BigInteger h;
        public ECPoint G;

        public FpCurve Curve;

        public BN254()
        {
            p = new BigInteger("16798108731015832284940804142231733909889187121439069848933715426072753864723");
            a = new BigInteger("0");
            b = new BigInteger("2");
            q = new BigInteger("16798108731015832284940804142231733909759579603404752749028378864165570215949");
            h = new BigInteger("1");
            Curve = new FpCurve(p, a, b, q, h);
            G = Curve.CreatePoint(new BigInteger("16798108731015832284940804142231733909889187121439069848933715426072753864722"), new BigInteger("1"));
        }
    }

    /// <summary>
    /// </summary>
    class ThresholdSignature
    {
        private int threshold;
        private int nParties;
        private BN254 curve;

        public ThresholdSignature(int _threshold, int _nParties)
        {
            threshold = _threshold;
            nParties = _nParties;
            curve = new BN254();
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
            for (ulong c = 1; c < (ulong)nParties + 1; c++)
                // sks[c-1] = Native.Instance.EvaluatePolynomial(ZRs, new BigInt(c));
                sks.Add(EvaluatePolynomial(ZRs, new BigInt(c)));

            // Generating verification keys.
            AltBn128G2 vk = Native.Instance.ScalarMulG2(secret);
            List<AltBn128G2> vks = new();

            foreach (BigInt sk in sks)
                vks.Add(Native.Instance.ScalarMulG2(sk));

            // Secret message.
            BigInt msg = new BigInt(123123123);
            AltBn128G1 msgHash = Native.Instance.ScalarMulG1(msg);

            // Generating shares of the message.
            List<AltBn128G1> shares = new();
            foreach(BigInt sk in sks)
                shares.Add(Native.Instance.ScalarPointMulG1(msgHash, sk));

            // Validating shares.
            // for (int c = 0; c < nParties; c++) {
            //     Console.WriteLine(Native.Instance.PairsEqual(msgHash, vks[c], shares[c], G2));
            // }

            // VerificationKey verificationKey = new(threshold, vk, vks);
            // List<SecretKey> secretKeys = new();

            // foreach(BigInteger sk in sks)
            //     secretKeys.Add(new SecretKey(sk));

            // return (new VerificationKey(threshold, vk, vks), new List<SecretKey>());
            ECPoint xx = curve.G.Multiply(BigInteger.One);
            return (new VerificationKey(0, xx, new List<ECPoint>()), new List<SecretKey>());
        }

        private BigInt EvaluatePolynomial(List<BigInt> coefficients, BigInt x)
        {
            BigInt result = new BigInt(0);
            for (int c = 0; c < coefficients.Count; c++) {
                // x * y + coef, and we reduce.
                result = Native.Instance.ModOrder(BigInt.Add(result, BigInt.Add(coefficients[c], BigInt.Multiply(x, result))));
            }
            return result;
        }
    }

    /// <summary>
    /// </summary>
    class VerificationKey
    {
        private int threshold;
        private ECPoint vk;
        private List<ECPoint> vks;
        private BN254 curve;

        public VerificationKey(int _threshold, ECPoint _vk, List<ECPoint> _vks) {
            threshold = _threshold;
            vk = _vk;
            vks = _vks;
            curve = new BN254();
        }

        private byte[] HashString(string input) {
            IDigest digest = new Sha256Digest();
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] result = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, 0, data.Length);
            digest.DoFinal(result, 0);
            return result;
        }

        private ECPoint HashToG1(string message)
        {
            // TODO Also initialize a BN254 curve for this class.
            // var curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256k1");
            ECPoint g1 = curve.G.Normalize();
            // ECPoint g2 = curve.G.Normalize().Twice().Normalize();
            byte[] hash = HashString(message);
            BigInteger x = new BigInteger(1, hash);
            BigInteger k = x.Mod(curve.q);

            // return curve.G.Multiply(k);
            return g1.Multiply(k);
        }

        /// <summary>
        /// </summary>
        private void HashFunction()
        {

        }

        /// <summary>
        /// </summary>
        private void Lagrange()
        {

        }

        /// <summary>
        /// </summary>
        private void VerifyShare()
        {

        }

        /// <summary>
        /// </summary>
        private void VerifySignature()
        {

        }

        /// <summary>
        /// </summary>
        private void CombineShares()
        {

        }

        /// <summary>
        /// </summary>
        private void HashMessage()
        {

        }
    }

    /// <summary>
    /// </summary>
    class SecretKey
    {
        private BigInteger sk;

        public SecretKey(BigInteger _sk)
        {
            sk = _sk;
        }

        /// <summary>
        /// </summary>
        private BigInteger GenerateShare(BigInteger MsgHash)
        {
            return MsgHash.Multiply(sk);
        }
    }
}
