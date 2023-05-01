using Xunit;
using Aurem.Nodes;
using Aurem.Units;
using Aurem.chDAGs;
using Aurem.Networking;
using Aurem.Randomness;

namespace Aurem.Tests
{
    public class TestFixture
    {
        private int numNodes = 10;
        private int minNodes = 7;
        private Network network;
        public List<Node> Nodes { get; private set; }

        public TestFixture()
        {
            // Initializing a network.
            network = new();

            // Generating keys.
            ThresholdSignature ts = new(minNodes, numNodes);
            (VerificationKey vk, List<SecretKey> sks) = ts.GenerateKeys();

            // Setting up nodes.
            Nodes = new List<Node>();
            for (int c = 0; c < numNodes; c++) {
                Nodes.Add(new Node((long) c, network, sks[c], vk));
            }
        }

        public void TestCHDAG()
        {

        }
    }
}
