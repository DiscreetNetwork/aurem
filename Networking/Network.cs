using System.Collections.Concurrent;
using NUlid;
using Aurem.Nodes;
using Aurem.Units;
using Aurem.Shared;

/// <summary>
/// </summary>
namespace Aurem.Networking
{
    /// <summary>
    /// This class simulates some operations that would be needed by an actual
    /// network.
    /// </summary>
    public class Network
    {
        private List<Node> _nodes;
        private ConcurrentDictionary<Ulid, Dictionary<Ulid, Ulid>> _byzantine;

        public Network()
        {
            _nodes = new();
            _byzantine = new();
        }

        /// <summary>
        /// Add adds a node to this network.
        /// </summary>
        public void Add(Node node)
        {
            _nodes.Add(node);
        }

        /// <summary>
        /// AddByzantine keeps a log of all the invalid units detected in the
        /// network, along with the owner of that unit and what node detected
        /// the anomaly.
        /// </summary>
        public void AddByzantine(Ulid reporterId, Unit unit)
        {
            if (!_byzantine.ContainsKey(reporterId))
                _byzantine[reporterId] = new Dictionary<Ulid, Ulid>();
            _byzantine[reporterId][unit.CreatorId] = unit.Id;
        }

        /// <summary>
        /// ReportByzantine prints a list of the Byzantine units found by the
        /// network of nodes.
        /// </summary>
        public void ReportByzantine()
        {
            foreach (Ulid reporterId in _byzantine.Keys) {
                foreach (KeyValuePair<Ulid, Ulid> byzantine in _byzantine[reporterId]) {
                    string msg = $"Node {reporterId} detected Byzantine unit {byzantine.Value} received from node {byzantine.Key}";
                    Console.WriteLine(msg);
                }
            }
        }

        /// <summary>
        /// GetNodes retrieves all the nodes that are participating in the
        /// network.
        /// </summary>
        public List<Node> GetNodes ()
        {
            return _nodes;
        }

        /// <summary>
        /// ShuffleNodes shuffles the order of the list of nodes in the network.
        /// The purpose of this operation is to aid in the simulation of
        /// latency and the asynchronous nature of the algorithm.
        /// </summary>
        public void ShuffleNodes() {
            Utils<Node>.Shuffle(_nodes);
        }

        public int NodesCount => _nodes.Count;

        /// <summary>
        /// MinimumParents calculates the minimum of parents needed for a unit,
        /// using the provided value.
        /// </summary>
        public int MinimumParents(int numNodes) {
            return (int)double.Round(numNodes - (numNodes - 1) / 3.0);
        }

        /// <summary>
        /// MinimumParents calculates the minimum of parents needed for a unit,
        /// using the current node count in the network.
        /// </summary>
        public int MinimumParents() {
            return MinimumParents(NodesCount);
        }
    }
}
