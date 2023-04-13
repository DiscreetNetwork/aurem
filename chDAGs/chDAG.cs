using System.Collections.Concurrent;
using NUlid;
using DotNetGraph;
using DotNetGraph.Node;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using Aurem.Units;
using Aurem.ECC;
using Aurem.ECC.Native;
using Aurem.Nodes;
using Aurem.Networking;
using Aurem.Shared;

/// <summary>
/// chDAG is a specific case of a DAG, where the storage of units is bound by
/// restrictions that facilitate a network of nodes to achieve consensus
/// <summary>
namespace Aurem.chDAGs
{
    public class chDAG
    {
        private readonly object _lock = new object();
        private readonly static object _ecc_lock = new object();
        private ConcurrentDictionary<int, List<Unit>> _units;
        private Node _owner;
        private Ulid _id;
        private ConcurrentDictionary<int, Unit> _heads;
        private List<Unit> _linord;

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
            _heads = new();
            _linord = new();
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
                        if (unit.Round != 0 && unit.Round < Round-3 && !IsValidUnit(unit)) {
                            // Reporting the byzantine unit.
                            _network.AddByzantine(_owner.Id, unit);
                            // FIXME Commenting this so it gets added anyway.
                            // We'll just report the unit for now.
                            // return;
                        }
                        if (!_units[round].Contains(unit)) {
                            _units[round].Add(unit);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// We poll the network to check if we have enough parents to create a
        /// new unit.
        /// </summary>
        public bool IsMinimumParents()
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
            // TODO Refactor this.
            if (Round == 0) {
                unit.Round = Round;
                if (!_units.ContainsKey(Round)) {
                    _units[Round] = new List<Unit>();
                }
                _units[Round++].Add(unit);
                return;
            }

            unit.Round = Round;
            unit.Parents = GetParents();
            if (!_units.ContainsKey(Round)) {
                _units[Round] = new List<Unit>();
            }

            // // Simulating malicious node.
            // Random random = new Random();
            // if (random.NextDouble() < 0.05) {
            //     Unit forgedUnit = new Unit(_owner.Id,
            //                                new byte[1]{ (byte)random.Next(0, 255) },
            //                                Native.Instance.ScalarMulG1(new BigInt(Round)));
            //     forgedUnit.Round = Round;
            //     _units[Round++].Add(forgedUnit);
            // } else {
            //     _units[Round++].Add(unit);
            // }
            _units[Round].Add(unit);

            LinearOrdering();
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
        public Unit ChooseHead(int round)
        {
            // We need at least two rounds to cast a vote.
            if (round > Round-2)
                return new Unit(-1, new byte[0], Native.Instance.ScalarMulG1(new BigInt(0)));
            // The node's unit at Round-0 will never be visible by any other node,
            // as it has not been broadcasted yet.
            // The units at Round-1 needed parents from Round-2 in order to be
            // created (2f+1 units).
            // Thus, the head unit is retrieved from Round-2.
            List<Unit> backers = GetRoundUnits(round+1);
            List<Unit> candidates = GetRoundUnits(round).ToList();
            int minParents = _network.MinimumParents();

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
                if (c >= minParents && candidate.Parents != null)
                    return candidate;
            }

            // Voting with CommonVote.
            // TODO The backers in this case need to be from round+4.
            if (backers.Count >= minParents) {
                Dictionary<long, AltBn128G1> shares = new();
                AltBn128G1 signature = new();
                foreach (Unit backer in backers)
                    shares[backer.CreatorId] = backer.Share;

                if (shares.Count > 0)
                    lock (_ecc_lock) signature = _owner.CombineShares(shares);

                lock (_ecc_lock)
                    if (_owner.SecretBit(signature) &&
                        _owner.ValidateSignature(signature, round+1))
                        // TODO Implement round units permutation.
                        return candidates[0];
            }

            return new Unit(-1, new byte[0], Native.Instance.ScalarMulG1(new BigInt(0)));
        }

        /// <summary>
        /// LinearOrdering sorts a subset of the units in the chDAG in an order
        /// that is going to be common to all the other chDAGs in the network.
        /// It orders a subset, because not all the units are going to be
        /// present in the local chDAG, due to the asynchronous nature of the
        /// protocol.
        /// </summary>
        public void LinearOrdering()
        {
            for (int c = 0; c < _units.Count; c++) {
                Unit head = ChooseHead(c);
                if (head.CreatorId != -1) {
                    _heads[c] = head;
                    if (head.Parents != null) {
                        // NOTE We can not create a copy of the parents. We're
                        // creating a copy for demonstrating the differences in
                        // structures in the saved plots/graphs.
                        List<Unit> parents = head.Parents.ToList();
                        parents.Sort((x, y) => x.Id.CompareTo(y.Id));
                        foreach (Unit unit in parents)
                            _linord.Add(unit);
                        _linord.Add(head);
                    }
                }
            }
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
            if (unit.Parents == null || unit.Parents.Count == 0)
                return false;
            int round = unit.Round;
            // Checking if the previous round in the local chDAG contains all of
            // the unit's parents, or if any sibling of this unit knows the parents.
            int confirmedCount = 0;
            foreach (Unit parent in unit.Parents) {
                // Checking if we have the actual unit in the local chDAG.
                if (_units[round-1].Any(x => x.Id == parent.Id)) {
                    confirmedCount++;
                    continue;
                }
                // Checking if any other unit in the same round has a reference
                // to the parent. This means that that node has the unit in its
                // local chDAG.
                foreach (Unit sibling in _units[round]) {
                    // NOTE We don't need to check if sibling == unit, because
                    // we haven't added the unit yet.
                    if (sibling.Parents == null) continue;
                    if (sibling.Parents.Any(x => x.Id == parent.Id)) {
                        confirmedCount++;
                        break;
                    }
                }
            }
            // Check if we have at least references to 2f+1 parents.
            return confirmedCount >= _network.MinimumParents();
        }

        /// <summary>
        /// UnitToDotNode creates a node for a DotGraph representing the
        /// provided chDAG unit.
        /// </summary>
        private DotNode UnitToDotNode(Unit unit, bool isHead)
        {
            // TODO Display something more meaningful on the unit.
            string unitId = $"{unit.CreatorId} {unit.Round} [ {unit.Data[0]} ]";

            System.Drawing.Color color = System.Drawing.Color.White;
            if (isHead)
                color = System.Drawing.Color.Purple;

            DotNode node = new DotNode(unitId)
            {
                Shape = DotNodeShape.Ellipse,
                Label = unitId,
                Color = color,
                FillColor = color,
                FontColor = color,
                Style = DotNodeStyle.Solid,
                Width = 0.5f,
                Height = 0.5f,
                PenWidth = 1.5f
            };

            node.SetCustomAttribute("rank", unit.Round.ToString());

            return node;
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
                for (int i = 0; i < _units[c].Count; i++) {
                    bool isHead = false;
                    if (_heads.ContainsKey(c) && _units[c][i].Id.Equals(_heads[c].Id))
                        isHead = true;
                    Unit unit = _units[c][i];
                    DotNode node = UnitToDotNode(unit, isHead);
                    nodes[unit.Id] = node;
                    graph.Elements.Add(node);

                    // Adding edges to parents.
                    if (unit.Parents != null)
                        foreach (Unit parent in unit.Parents) {
                            if (nodes.ContainsKey(parent.Id)) {
                                System.Drawing.Color color = System.Drawing.Color.Violet;
                                if (_heads.ContainsKey(c-1) && _heads[c-1].Id == parent.Id)
                                    color = System.Drawing.Color.Red;
                                graph.Elements.Add(UnitsEdge(nodes[parent.Id], node, color));
                            }
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
