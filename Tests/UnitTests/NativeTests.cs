using System.Numerics;
using Xunit;
using Aurem.ECC;
using Aurem.ECC.Native;

namespace Aurem.Tests
{
    [Collection("Tests")]
    public class ECCTests
    {
        public ECCTests()
        {
            Native.Instance.Init();
        }

        [Fact]
        public void TestAltBn128Order()
        {
            Assert.Equal(BigInt.ToBigInteger(Native.Instance.Order()),
                         BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617"));
        }

        [Fact]
        public void TestAltBn128G1()
        {
            AltBn128G1 G1 = Native.Instance.G1();
            Assert.Equal(BigInt.ToBigInteger(G1.X), BigInteger.Parse("1"));
            Assert.Equal(BigInt.ToBigInteger(G1.Y), BigInteger.Parse("2"));
            Assert.Equal(BigInt.ToBigInteger(G1.Z), BigInteger.Parse("1"));
        }
    }
}
