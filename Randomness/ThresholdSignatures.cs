using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

/// <summary>
/// Implementation of components needed to achieve Common Randomness for Common
/// Vote among the nodes in Aurem.
/// </summary>
namespace Aurem.Randomness
{
    /// <summary>
    /// </summary>
    class ThresholdSignature
    {
        /// <summary>
        /// GenerateKeys
        /// </summary>
        public (VerificationKey vkey, List<SecretKey> skeys) GenerateKeys(int threshold, int nParties)
        {
            SecureRandom secureRandom = new SecureRandom();

            int bitLength = 512;
            BigInteger q = new BigInteger("19387465239847239847");

            // Generating a set of coefficients.
            List<BigInteger> ZRs = new();
            for (int c = 0; c < threshold; c++) {
                BigInteger r = new BigInteger(bitLength, secureRandom);
                ZRs.Add(r.Mod(q));
            }
            BigInteger secret = ZRs[ZRs.Count-1];

            // Generating secret keys.
            List<BigInteger> sks = new();
            for (int c = 1; c < nParties + 1; c++)
                sks.Add(EvaluatePolynomial(ZRs, BigInteger.ValueOf(c)));

            // Specify the curve to use for key generation.
            // TODO What curve should we use?
            var curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName("secp256r1");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

            // Generating verification keys.
            var vk = curve.G.Multiply(secret);
            List<Org.BouncyCastle.Math.EC.ECPoint> vks = new();
            foreach (BigInteger sk in sks)
                vks.Add(curve.G.Multiply(sk));

            // // Generate a new key pair
            // var random = new SecureRandom();
            // var keyPairGenerator = new ECKeyPairGenerator("ECDSA");
            // keyPairGenerator.Init(new ECKeyGenerationParameters(domain, random));
            // var keyPair = keyPairGenerator.GenerateKeyPair();

            // // Extract the public and private key components
            // var publicKey = (ECPublicKeyParameters)keyPair.Public;
            // var privateKey = (ECPrivateKeyParameters)keyPair.Private;

            return (new VerificationKey(), new List<SecretKey>());
        }

        private BigInteger EvaluatePolynomial(List<BigInteger> coefficients, BigInteger x)
        {
            BigInteger result = BigInteger.ValueOf(0);
            for (int c = 0; c < coefficients.Count; c++) {
                // x * y + coef, and we reduce.
                result.Add(coefficients[c].Add(x.Multiply(result)));
            }
            return result;
        }
    }

    /// <summary>
    /// </summary>
    class SecretKey
    {
        /// <summary>
        /// </summary>
        private void GenerateShare()
        {

        }
    }

    /// <summary>
    /// </summary>
    class VerificationKey
    {
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
}
