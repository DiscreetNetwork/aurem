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
            Assert.Equal(BigInt.ToBigInteger(Native.Instance.Order()), BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617"));
        }

        [Fact]
        public void TestAltBn128G1()
        {
            AltBn128G1 G1 = Native.Instance.G1();
            Assert.Equal(BigInt.ToBigInteger(G1.X), BigInteger.Parse("1"));
            Assert.Equal(BigInt.ToBigInteger(G1.Y), BigInteger.Parse("2"));
            Assert.Equal(BigInt.ToBigInteger(G1.Z), BigInteger.Parse("1"));
        }

        [Fact]
        public void TestScalarMul()
        {
            BigInt scalar = new BigInt(5);
            BigInteger G1X = BigInteger.Parse("156436179618191616");
            BigInteger G1Y = BigInteger.Parse("165481912828487805829373952");
            BigInteger G1Z = BigInteger.Parse("445823664");

            // G1.
            AltBn128G1 pointG1 = Native.Instance.ScalarMulG1(scalar);
            Assert.Equal(BigInt.ToBigInteger(pointG1.X), G1X);
            Assert.Equal(BigInt.ToBigInteger(pointG1.Y), G1Y);
            Assert.Equal(BigInt.ToBigInteger(pointG1.Z), G1Z);

            // G2.
            BigInteger G2X = BigInteger.Parse("19769190693372437764891708404815307763739844405371328100770824290829375973365");
            BigInteger G2Y = BigInteger.Parse("6533349467797685961836086508588911858227855513259962928558343469643175403678");
            BigInteger G2Z = BigInteger.Parse("21859112873699330112740160736260475942708944652034209147320887203148072131504");
            AltBn128G2 pointG2 = Native.Instance.ScalarMulG2(scalar);
            Assert.Equal(BigInt8.ToBigInteger(pointG2.X), G2X);
            Assert.Equal(BigInt8.ToBigInteger(pointG2.Y), G2Y);
            Assert.Equal(BigInt8.ToBigInteger(pointG2.Z), G2Z);
        }

        [Fact]
        public void TestPointScalarMul()
        {
            BigInt scalar = new BigInt(5);

            // G1.
            AltBn128G1 _pointG1 = Native.Instance.ScalarMulG1(scalar);
            AltBn128G1 pointG1 = Native.Instance.ScalarPointMulG1(_pointG1, scalar);
            BigInteger G1X = BigInteger.Parse("13438055234505968371136474562186173420973714229863653175995600916593077789832");
            BigInteger G1Y = BigInteger.Parse("21369695463557511487682581508440364657717668048917395974613715493078402001692");
            BigInteger G1Z = BigInteger.Parse("2915416412093971481993167225784061972651748244338813502634230373877410041715");
            Assert.Equal(BigInt.ToBigInteger(pointG1.X), G1X);
            Assert.Equal(BigInt.ToBigInteger(pointG1.Y), G1Y);
            Assert.Equal(BigInt.ToBigInteger(pointG1.Z), G1Z);

            // G2.
            AltBn128G2 _pointG2 = Native.Instance.ScalarMulG2(scalar);
            AltBn128G2 pointG2 = Native.Instance.ScalarPointMulG2(_pointG2, scalar);
            BigInteger G2X = BigInteger.Parse("11351021384908969247483016026257360796510143727433085959225510072009874152440");
            BigInteger G2Y = BigInteger.Parse("2749958596933893909899951583629448809865433626499816714738394411216054014956");
            BigInteger G2Z = BigInteger.Parse("1518793293787788404567123212489975929527672212644088514583669955759759310623");
            Assert.Equal(BigInt8.ToBigInteger(pointG2.X), G2X);
            Assert.Equal(BigInt8.ToBigInteger(pointG2.Y), G2Y);
            Assert.Equal(BigInt8.ToBigInteger(pointG2.Z), G2Z);
        }

        [Fact]
        public void TestBigInt()
        {
            BigInt n = new BigInt(123123123);
            Assert.Equal(BigInt.ToBigInteger(new BigInt(123123123)), 123123123);
            Assert.Equal(BigInt.ToBigInteger(BigInt.Add(n, n)), 246246246);
            Assert.Equal(BigInt.ToBigInteger(BigInt.Multiply(n, new BigInt(2))), 246246246);
        }
    }
}
