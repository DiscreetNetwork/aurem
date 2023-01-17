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

        public Network(){
            _nodes = new();
        }

        public void Add(Node node) {
            _nodes.Add(node);
        }

        public int NodesCount => _nodes.Count;

        public int MinimumParents() {
            return (int)double.Round(NodesCount - (NodesCount - 1) / 3.0);
        }
    }
}
