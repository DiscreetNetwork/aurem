using Xunit;
using Aurem.Nodes;
using Aurem.Units;
using Aurem.Networking;
using Aurem.Randomness;

namespace Aurem.Tests
{
    [Collection("Tests")]
    public class NodeTests
    {
        private int numNodes = 10;
        private int minNodes = 7;
        private ThresholdSignature ts;
        private Node node;

        public NodeTests()
        {
            ts = new(minNodes, numNodes);
            (VerificationKey vk, List<SecretKey> sks) = ts.GenerateKeys();
            node = new Node(0, new Network(), sks[0], vk);
        }

        [Fact]
        public void TestNode()
        {
            node.CreateUnit(new byte[1]{ 3 });
            chDAGs.chDAG chDAG = node.GetChDAG();
            List<Unit> units = chDAG.GetRoundUnits(0);
            Assert.Single(units);
            Assert.Equal(units[0].Data, new byte[1]{ 3 });
        }
    }
}
