using System.Collections.Concurrent;
using DotNetGraph;
using DotNetGraph.Node;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using Aurem.Units;
using Aurem.Nodes;
using Aurem.Networking;

/// <summary>
/// chDAG is a specific case of a DAG, where the storage of units is bound by
/// restrictions that facilitate a network of nodes to achieve consensus
/// <summary>
namespace Aurem.chDAGs
{
    public class chDAG
    {
        private ConcurrentDictionary<int, List<Unit>> _units;
        private Node _owner;

        public int Round { get; set; } = 0;
        /// <summary>
        /// _network defines the network to which this chDAG is syncing to.
        /// </summary>
        private Network _network;

        public chDAG(Network network, Node owner)
        {
            _units = new();
            _owner = owner;
            _network = network;
        }

        /// <summary>
        /// Sync syncs the chDAG to the latest units broadcasted by the network.
        /// </summary>
        private void Sync() {
            foreach (Node node in _network.GetNodes()) {
                if (!node.Id.Equals(this._owner.Id)) {
                    chDAG chDAG = node.GetChDAG();
                    // NOTE We're checking the count first, to avoid an error in chDAG.GetRoundUnits()
                    // that occurs when trying to access a non-existent key. An alternative is to return
                    // an empty List<Unit>, but I don't know if this would be inefficient, resource-wise.
                    if (chDAG.GetRoundUnitsCount(Round-1) < 1) continue;

                    // NOTE Checking that a node doesn't sync with itself wasn't enough for some reason.
                    // FIXME A theory is that nodes can end up sharing the same List object.
                    List<Unit> units = chDAG.GetRoundUnits(Round-1);
                    if (units.GetHashCode() == _units[Round-1].GetHashCode()) continue;

                    foreach (Unit unit in units) {
                        if (!_units[Round-1].Contains(unit))
                            _units[Round-1].Add(unit);
                    }
                }
            }
        }

        // The number of required parents when creating a new unit in the DAG.
        // The minimum should be equal to (N-(N-1)/3), derived from the formula
        // N = 3f+1, where f is the number of Byzantine nodes and N is the total
        // number of node creators.

        /// <summary>
        /// We poll the network to check if we have enough parents to create a
        /// new unit.
        /// </summary>
        private bool IsMinimumParents()
        {
            return _units[Round-1].Count >= _network.MinimumParents();
        }

        /// <summary>
        /// Add adds a new unit to the chDAG, ensuring that it has parents from
        /// the previous round and updates the units round to the current chDAG
        /// round.
        /// </summary>
        public void Add(Unit unit)
        {
            unit.Round = Round;
            // We need to check if this is a new instance of a DAG.
            // If it's new, then a new unit cannot have parents.
            if (Round != 0) {
                bool isEnoughNodes = IsMinimumParents();
                while (!isEnoughNodes) {
                    if(isEnoughNodes) {
                        unit.Parents = (List<Unit>)_units[Round-1].Take(_network.MinimumParents());
                    } else {
                        Sync();
                        isEnoughNodes = IsMinimumParents();
                    };
                    // Thread.Yield();
                    Task.Yield();
                }
            }

            if (!_units.ContainsKey(Round)) {
                _units[Round] = new List<Unit>();
            }

            _units[Round].Add(unit);
            Round++;
        }

        private int GetRoundUnitsCount(int round)
        {
            return _units.ContainsKey(round) ? _units[round].Count : 0;
        }

        /// <summary>
        /// GetRoundUnits retrieves the units belonging to certain round.
        /// </summary>
        public List<Unit> GetRoundUnits(int round)
        {
            return _units[round];
        }

        /// <summary>
        /// GetParents needs to return the minimum required number of parents to
        /// avoid a Byzantine attack. The units need to belong to the previous
        /// round number.
        /// <summary>
        public List<Unit> GetParents()
        {
            // TODO
            // Task.Delay(1000);
            return new List<Unit>();
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
    }
}
