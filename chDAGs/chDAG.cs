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
                        if (!IsValidUnit(unit))
                            // TODO Discuss what we should do with Byzantine nodes.
                            throw new System.Exception("Possible Byzantine unit received.");
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
                // unit.Parents = _units[Round-1].Take(_network.MinimumParents()).ToList();
                unit.Parents = GetParents();
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
                for (int c = Round - 4; c < Round && c > 0; c++)
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
        /// GetParents returns the latest units produced by each node in the
        /// network. Note that this method does not have the responsibility of
        /// checking if there are at least 2f+1 units in Round-1.
        /// <summary>
        public List<Unit> GetParents()
        {
            // We start with all the units in the previous round.
            List<Unit> units = _units[Round-1].ToList();
            for (int c = Round-2; c >= 0; c--) {
                int addedCount = 0;
                // Checking if we can add a unit from a node that has not
                // provided a unit to the list of parents.
                foreach (Unit unit in _units[c]) {
                    if (!units.Any(x => x.CreatorId == unit.CreatorId)) {
                        units.Add(unit);
                        addedCount++;
                    }
                }
                // If addedCount == 0, then it means that we now have units from
                // all the nodes.
                if (addedCount == 0) break;
            }
            return units;
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
        /// IsValidUnit checks if the parents of the provided unit are present
        /// in the local chDAG, and that there is a minimum of 2f+1 parents from
        /// the previous round.
        /// </summary>
        public bool IsValidUnit(Unit unit)
        {
            // If round == 0, then no parents are required.
            if (unit.Round == 0)
                return true;
            // If no parent, the unit is obviously invalid.
            if (unit.Parents == null)
                return false;
            int round = unit.Round;
            // Checking if the previous round in the local chDAG contains all of
            // the unit's parents.
            foreach (Unit parent in unit.Parents) {
                if (!_units[round-1].Any(x => x.Id == parent.Id))
                    return false;
            }
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
        private DotEdge UnitsEdge(DotNode node1, DotNode node2, System.Drawing.Color color)
        {
            return new DotEdge(node1, node2)
            {
                ArrowHead = DotEdgeArrowType.Diamond,
                ArrowTail = DotEdgeArrowType.Empty,
                Color = color,
                FontColor = System.Drawing.Color.White,
                Label = "",
                Style = DotEdgeStyle.Solid,
                PenWidth = 1.0f
            };
        }

        /// <summary>
        /// Creates a PNG of the chDAG and saves it to the specified path.
        /// </summary>
        public void Save(string path)
        {
            Random random = new Random();
            var graph = new DotGraph("chDAG", true);

            Dictionary<Ulid, DotNode> nodes = new();

            // Adding units (nodes, in graph theory).
            for (int c = 0; c < Round; c++) {
                // We want one of the units to have its edges in a different color,
                // so it's more noticeable what's happening.
                int diffColorNode = random.Next(0, _units[c].Count);

                for (int i = 0; i < _units[c].Count; i++) {
                    Unit unit = _units[c][i];
                    DotNode node = UnitToDotNode(unit);
                    System.Drawing.Color color = System.Drawing.Color.Violet;
                    if (diffColorNode == i)
                        color = System.Drawing.Color.Red;
                    nodes[unit.Id] = node;
                    graph.Elements.Add(node);

                    // Adding edges to parents.
                    if (unit.Parents != null)
                        foreach (Unit parent in unit.Parents) {
                            if (nodes.ContainsKey(parent.Id))
                                graph.Elements.Add(UnitsEdge(nodes[parent.Id], node, color));
                        }
                }
            }

            string dot = graph.Compile(true);
            // There isn't a way to determine the layout of the Graphviz graph in DotNetGraph.
            // A quick hack is to insert the option directly.
            dot = dot.Insert(dot.IndexOf("\n") + 1, "rankdir=LR;\n");
            dot = dot.Insert(dot.IndexOf("\n") + 1, "bgcolor=black;\n");
            // Saving to file.

            Random rand = new Random();
            File.WriteAllText(Path.Combine(path, $"{this._id}_{Ulid.NewUlid()}.dot"), dot);
        }
    }
}
