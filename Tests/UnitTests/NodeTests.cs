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
        private TestFixture fix = new();

        public NodeTests()
        {

        }

        public async Task AddUnit(Node node, byte data)
        {
            node.CreateUnit(new byte[1]{ data });
        }

        [Fact]
        public async void TestNode()
        {
            int timeoutMilliseconds = 1000;
            Node node = fix.Nodes[0];
            // Testing for round 0.
            // node.CreateUnit(new byte[1]{ 3 });
            AddUnit(node, 3);
            chDAGs.chDAG chDAG = node.GetChDAG();
            List<Unit> units = chDAG.GetRoundUnits(0);
            Assert.Single(units);
            Assert.Equal(units[0].Data, new byte[1]{ 3 });
            // Testing for round 1.
            // Task unitTimeout = AddUnit(node, 5);
            Task completedTask = await Task.WhenAny(AddUnit(node, 5), Task.Delay(timeoutMilliseconds));
            Assert.NotSame(unitTimeout, completedTask);

            // node.CreateUnit(new byte[1]{ 5 });
            // // node.CreateUnit(new byte[1]{ 5 });
            // // node.CreateUnit(new byte[1]{ 5 });
            // // node.CreateUnit(new byte[1]{ 5 });
            // units = chDAG.GetRoundUnits(3);
            // Assert.Single(units);
            // Assert.Equal(units[0].Data, new byte[1]{ 5 });
            // // Creating units
            // for (int c = 0; c < fix.Nodes.Count; c++) {
            //     fix.Nodes[c].CreateUnit(new byte[1]{ 1 });
            // }
        }
    }
}
