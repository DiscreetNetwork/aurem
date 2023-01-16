using System.Collections;
// using System.Collections.Concurrent;
// using System.Linq;
using NUlid;
using DotNetGraph;
using DotNetGraph.Node;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
// using System.Threading.Tasks;

namespace Aurem
{
    /// <summary>
    /// A node in Aleph represents an entity that is producing DAG units.
    /// </summary>
    public class Node
    {
        private Ulid _id;
        private int _round = 0;
        private chDAG<Unit> _chDAG;

        public Node(Ulid id)
        {
            _id = id;
            _chDAG = new chDAG<Unit>();
        }

        public void CreateUnit(byte[] data)
        {
            Unit unit;
            // We need to check if this is a new instance of a DAG.
            // If it's new, then a new unit cannot have parents.
            if (_chDAG.Count != 0) {
                // TODO We need to wait until we have enough units from other nodes.
                // For now, let's just have empty parents.
                // _dag.GetParents();
                // Unit unit = new(parents, _round, data);
                unit = new(new List<Unit>(), _round, data);
            }
            else {
                unit = new(new List<Unit>(), _round, data);
            }
            // TODO We need to retrieve the round number from the network.
            // What happens if you're a new node? You need to ask the network
            // what's the current round.
            _round++;
            _chDAG.Add(unit);
        }
    }

    public class Unit
    {
        public byte[] Data { get; set; }
        public List<Unit> Parents { get; set; }
        public int Round { get; set; }

        public Unit(List<Unit> parents, int round, byte[] data)
        {
            this.Parents = parents;
            this.Data = data;
            this.Round = round;
        }
    }

    /// <summary>
    /// This class simulates some operations that would be needed by an actual
    /// network.
    /// </summary>
    public static class Network
    {
        public static int NodesCount { get; set; } = 10;
        public static int MinimumParents() {
            return (int)double.Round(Network.NodesCount - (Network.NodesCount - 1) / 3.0);
        }
    }

    public class DAG<NodeType> : ISet<NodeType>
    {
        // The number of required parents when creating a new unit in the DAG.
        // The minimum should be equal to (N-(N-1)/3), derived from the formula
        // N = 3f+1, where f is the number of Byzantine nodes and N is the total
        // number of node creators.
        private int _numParents;
        private HashSet<NodeType> _units;

        public DAG()
        {
            _numParents = 5;
            _units = new HashSet<NodeType>();
        }

        public bool Add(NodeType unit) {
            return _units.Add(unit); // TODO
        }

        public bool Add(IEnumerable<NodeType> units) {
            return true; // TODO
        }

        /// <summary>
        /// Creates a PNG of the DAG.
        /// </summary>
        public void Save() {
            var graph = new DotGraph("DAG");

            var myNode = new DotNode("Node")
            {
                Shape = DotNodeShape.Ellipse,
                Label = "My node!",
                FillColor = System.Drawing.Color.Coral,
                FontColor = System.Drawing.Color.Black,
                Style = DotNodeStyle.Dotted,
                Width = 0.5f,
                Height = 0.5f,
                PenWidth = 1.5f
            };

            // Add the node to the graph
            graph.Elements.Add(myNode);

            // Create an edge with identifiers
            // var myEdge = new DotEdge("myNode1", "myNode2");

            // Create an edge with nodes and attributes
            var myEdge = new DotEdge(myNode, myNode)
            {
                ArrowHead = DotEdgeArrowType.Box,
                ArrowTail = DotEdgeArrowType.Diamond,
                Color = System.Drawing.Color.Red,
                FontColor = System.Drawing.Color.Black,
                Label = "My edge!",
                Style = DotEdgeStyle.Dashed,
                PenWidth = 1.5f
            };

            // Add the edge to the graph
            graph.Elements.Add(myEdge);

            var dot = graph.Compile(true);
            File.WriteAllText("graph.dot", dot);
            // Run it yourself in a terminal...
            // Process.Start("dot", "-Tpng -o graph.png graph.dot").WaitForExit();
        }

        // Sigh...
        public int Count => _units.Count;
        public IEnumerator<NodeType> GetEnumerator() => _units.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _units.GetEnumerator();
        public bool IsReadOnly => false;
        public bool Remove(NodeType unit) => true; // Do we even need this? In case yes: TODO
        public void CopyTo(NodeType[] units, int index) => _units.CopyTo(units, index);
        public bool Contains(NodeType item) =>  _units.Contains(item);
        public void Clear() => _units.Clear();
        public void UnionWith(IEnumerable<NodeType> other) => this.Add(other);
        void ICollection<NodeType>.Add(NodeType item) => this.Add(item);
        public void SymmetricExceptWith(ICollection<NodeType> units) { }
        public void SymmetricExceptWith(IEnumerable<NodeType> units) { }
        public bool SetEquals(IEnumerable<NodeType> units) => _units.SetEquals(units);
        public bool Overlaps(IEnumerable<NodeType> units) => _units.Overlaps(units);
        public bool IsSupersetOf(IEnumerable<NodeType> units) => _units.IsSupersetOf(units);
        public bool IsSubsetOf(IEnumerable<NodeType> units) => _units.IsSubsetOf(units);
        public bool IsProperSubsetOf(IEnumerable<NodeType> units) => _units.IsProperSubsetOf(units);
        public bool IsProperSupersetOf(IEnumerable<NodeType> units) => _units.IsProperSupersetOf(units);
        public void IntersectWith(DAG<NodeType> dag) => this._units.IntersectWith(dag._units);
        public void IntersectWith(ICollection<NodeType> unit) {  }
        public void IntersectWith(IEnumerable<NodeType> unit) {  }
        public void ExceptWith(IEnumerable<NodeType> unit) {  }
    }

    public class chDAG<NodeType> : DAG<NodeType> {
        /// <summary>
        /// GetParents needs to return the minimum required number of parents
        /// to avoid a Byzantine attack.
        /// <summary>
        public List<NodeType> GetParents()
        {
            // TODO
            // Task.Delay(1000);
            return new List<NodeType>();
        }

        /// <summary>
        /// IsConsistent determines if every unit in current chDAG is valid. A
        /// unit being valid means that it must have certain number of parents,
        /// parents cannot be duplicated and each unit is included in the DAG
        /// only once.
        /// </summary>
        public bool IsConsistent()
        {
            // TODO
            return true;
        }
    }

    class Program
    {
        int numNodes = 10;

        private List<Node> InitNodes(int numNodes)
        {
            List<Node> nodes = new List<Node>();
            for (int c = 0; c < numNodes; c++) {
                nodes.Add(new Node(Ulid.NewUlid()));
            }
            return nodes;
        }

        private void Run()
        {
            Random random = new Random();
            // chDAG<Unit> dag = new();

            // Creating nodes to simulate the "minting" of units.
            List<Node> nodes = InitNodes(numNodes);
            while (true) {
                // Now we'll create units for each node.
                // We are randomizing the order of each node for each iteration.
                // The purpose of this is to simulate an asynchronous behavior.
                foreach (Node node in nodes.OrderBy(node => random.Next())) {
                    double r = random.NextDouble();
                    // We want to simulate some latency. We'll do it in a very dumb way.
                    // There's a chance that a node "won't add" a unit in this
                    // "round" or, more precisely, it won't add it "in time".
                    if (r > 0.9)
                        // We don't care about what data we store for this PoC.
                        node.CreateUnit(new byte[1]);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Running");
            Program prgrm = new();
            prgrm.Run();
        }
    }

}
