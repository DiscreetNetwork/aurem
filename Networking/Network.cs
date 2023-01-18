using Aurem.Nodes;

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

        public Network()
        {
            _nodes = new();
        }

        /// <summary>
        /// Add adds a node to this network.
        /// </summary>
        public void Add(Node node)
        {
            _nodes.Add(node);
        }

        /// <summary>
        /// GetNodes retrieves all the nodes that are participating in the
        /// network.
        /// </summary>
        public List<Node> GetNodes ()
        {
            return _nodes;
        }

        public int NodesCount => _nodes.Count;

        public int MinimumParents() {
            return (int)double.Round(NodesCount - (NodesCount - 1) / 3.0);
        }
    }
}
