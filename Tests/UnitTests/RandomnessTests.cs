using Xunit;
using Aurem.Randomness;
using Aurem.ECC;
using Aurem.ECC.Native;

namespace Aurem.Tests
{
    [Collection("Tests")]
    public class RandomnessTests
    {
        private int numNodes = 10;
        private int minNodes = 7;
        private ThresholdSignature ts;

        public RandomnessTests()
        {
            Native.Instance.Init();
            ts = new(minNodes, numNodes);
        }

        [Fact]
        public void TestThresholdSignatures()
        {
            (VerificationKey vk, List<SecretKey> sks) = ts.GenerateKeys();

            Dictionary<long, AltBn128G1> shares = new();
            BigInt secret = new BigInt(5);
            for (long c = 0; c < minNodes; c++) {
                shares[c] = sks[(int)c].GenerateShare(secret);
                Assert.True(vk.VerifyShare(shares[c], c, secret));
            }

            AltBn128G1 signature = vk.CombineShares(shares);
            Assert.True(vk.VerifySignature(signature, secret));

            Dictionary<long, AltBn128G1> notEnoughShares = new();
            for (long c = 0; c < minNodes-1; c++) {
                notEnoughShares[c] = sks[(int)c].GenerateShare(secret);
            }
            AltBn128G1 invalidSignature = vk.CombineShares(notEnoughShares);
            Assert.False(vk.VerifySignature(invalidSignature, secret));
        }
    }
}
