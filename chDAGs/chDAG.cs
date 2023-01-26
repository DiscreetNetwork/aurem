using System.Collections.Concurrent;
using System.Linq;
using NUlid;
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
        private static readonly object _lock = new object();
        private ConcurrentDictionary<int, List<Unit>> _units;
        private Node _owner;
        private Ulid _id;

        public int Round { get; set; } = 0;
        /// <summary>
        /// _network defines the network to which this chDAG is syncing to.
        /// </summary>
        private Network _network;

        public chDAG(Network network, Node owner)
        {
            _id = Ulid.NewUlid();
            _units = new();
            _owner = owner;
            _network = network;
        }

        /// <summary>
        /// Sync syncs the chDAG to the latest units broadcasted by the network.
        /// </summary>
        public void Sync(int round)
        {
            // Checking if we already have all the possible units, assuming a
            // network of constant size.
            if (_units[round].Count == _network.NodesCount) return;
            // NOTE In the end, we'll be just receiving units asynchrously, from
            // whatever past round.
            foreach (Node node in _network.GetNodes()) {
                if (!node.Id.Equals(this._owner.Id)) {
                    chDAG chDAG = node.GetChDAG();
                    // NOTE We're checking the count first, to avoid an error in chDAG.GetRoundUnits()
                    // that occurs when trying to access a non-existent key. An alternative is to return
                    // an empty List<Unit>, but I don't know if this would be inefficient, resource-wise.
                    // Regarding the empty List, this should be very efficient, because we're just
                    // creating a pointer; we're not creating data.
                    if (chDAG.GetRoundUnitsCount(round) < 1) continue;

                    // NOTE Checking that a node doesn't sync with itself wasn't enough for some reason.
                    // FIXME A theory is that nodes can end up sharing the same List object.
                    List<Unit> units = chDAG.GetRoundUnits(round);
                    if (units.GetHashCode() == _units[round].GetHashCode()) continue;

                    foreach (Unit unit in units) {
                        if (!_units[round].Contains(unit))
                            _units[round].Add(unit);
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
            // TODO Refactor this.
            if (Round == 0) {
                if (!_units.ContainsKey(Round)) {
                    _units[Round] = new List<Unit>();
                }
                _units[Round].Add(unit);
                Round++;
                return;
            }
            // We need to check if this is a new instance of a DAG.
            // If it's new, then a new unit cannot have parents.
            if (Round != 0) {
                bool isEnoughNodes = IsMinimumParents();
                while (!isEnoughNodes) {
                    lock (_lock) Sync(Round-1);
                    isEnoughNodes = IsMinimumParents();
                }
                // TODO We need to capture a snapshot of all the latest
                // information we have from the chDAG, not just 2f+1 units from
                // r-1. If we haven't received some units from some nodes in
                // r-1, we have to use the latest units that we have received
                // from them.
                unit.Parents = _units[Round-1].Take(_network.MinimumParents()).ToList();
            }

            lock (_lock) {
                if (!_units.ContainsKey(Round)) {
                    _units[Round] = new List<Unit>();
                }

                // NOTE This will not be necessary later, as we should adopt a
                // publisher-subscriber architecture or something similar, where
                // we just listen to a publisher for new units, instead of requesting
                // for units from a particular round.
                // NOTE We're using -4 rounds, because this should be more than enough
                // for completely updating any missing past units for this PoC.
                for (int c = Round - 4; c < Round; c++)
                    Sync(c);

                _units[Round].Add(unit);
                Round++;
            }
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
        /// ChooseHead determines a unit that is visible by all the nodes
        /// registered to a network.
        /// </summary>
        public Unit ChooseHead()
        {
            // The node's unit at Round-0 will never be visible by any other node,
            // as it has not been broadcasted yet.
            // The units at Round-1 needed parents from Round-2 in order to be
            // created (2f+1 units).
            // Thus, the head unit is retrieved from Round-2.
            List<Unit> backers = GetRoundUnits(Round-1);
            List<Unit> candidates = GetRoundUnits(Round-2);

            if (backers == null || candidates == null)
                throw new System.Exception("Units of rounds Round-1 and Round-2 should exist.");

            // We first sort the parents by Id.
            candidates.Sort((x, y) => x.Id.CompareTo(y.Id));

            // We need to count the number of instances of a parent unit present
            // in units from the next round, i.e. the first unit, ordered by
            // age, with 2f+1 children wins.
            foreach (Unit candidate in candidates) {
                int c = 0;
                foreach (Unit backer in backers)
                    if (backer.Parents != null && backer.Parents.Contains(candidate))
                        c++;

                // Then this is the first unit in the sorted list of candidates
                // that is visible to all nodes.
                // NOTE It is a bit unclear if it must be visible to all nodes
                // or to 2f+1 nodes. We're taking the 2f+1 nodes route.
                if (c == _network.MinimumParents())
                if (candidate.Parents != null) {
                    return candidate;
                }
            }
            throw new System.Exception("Head unit could not be found.");
        }

        /// <summary>
        /// LinearOrdering sorts a subset of the units in the chDAG in an order
        /// that is going to be common to all the other chDAGs in the network.
        /// It orders a subset, because not all the units are going to be
        /// present in the local chDAG, due to the asynchronous nature of the
        /// protocol.
        /// </summary>
        private void LinearOrdering()
        {
            Unit head = ChooseHead();
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
        /// UnitToDotNode creates a node for a DotGraph representing the
        /// provided chDAG unit.
        /// </summary>
        private DotNode UnitToDotNode(Unit unit)
        {
            // TODO Display something more meaningful on the unit.
            string unitId = $"{unit.Round} [ {unit.Data[0]} ]";
            return new DotNode(unitId)
            {
                Shape = DotNodeShape.Ellipse,
                Label = unitId,
                Color = System.Drawing.Color.White,
                FillColor = System.Drawing.Color.White,
                FontColor = System.Drawing.Color.White,
                Style = DotNodeStyle.Solid,
                Width = 0.5f,
                Height = 0.5f,
                PenWidth = 1.5f
            };
        }

        /// <summary>
        /// UnitsEdge returns an edge between two units in a chDAG for a
        /// DotGraph.
        /// </summary>
        private DotEdge UnitsEdge(DotNode node1, DotNode node2)
        {
            return new DotEdge(node1, node2)
            {
                ArrowHead = DotEdgeArrowType.Diamond,
                ArrowTail = DotEdgeArrowType.Empty,
                Color = System.Drawing.Color.Violet,
                FontColor = System.Drawing.Color.White,
                Label = "",
                Style = DotEdgeStyle.Solid,
                PenWidth = 1.0f
            };
        }

        /// <summary>
        /// Creates a PNG of the chDAG and saves it to the specified path.
        /// </summary>
        public void Save(string path) {
            var graph = new DotGraph("chDAG", true);

            Dictionary<Ulid, DotNode> nodes = new();

            // Adding units (nodes, in graph theory).
            for (int c = 0; c < Round; c++) {
                foreach (Unit unit in _units[c]) {
                    DotNode node = UnitToDotNode(unit);
                    nodes[unit.Id] = node;
                    graph.Elements.Add(node);

                    // Adding edges to parents.
                    if (unit.Parents != null)
                        foreach (Unit parent in unit.Parents) {
                            if (nodes.ContainsKey(parent.Id))
                                graph.Elements.Add(UnitsEdge(nodes[parent.Id], node));
                        }
                }
            }

            string dot = graph.Compile(true);
            // There isn't a way to determine the layout of the Graphviz graph in DotNetGraph.
            // A quick hack is to insert the option directly.
            dot = dot.Insert(dot.IndexOf("\n") + 1, "rankdir=LR;\n");
            dot = dot.Insert(dot.IndexOf("\n") + 1, "bgcolor=black;\n");
            // Saving to file.

            File.WriteAllText(Path.Combine(path, $"{this._id}.dot"), dot);
        }
    }
}
