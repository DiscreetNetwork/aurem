using NUlid;
using Aurem.chDAGs;
using Aurem.Units;
using Aurem.Networking;

/// <summary>
/// Node represents a Unit creator; an entity responsible for creating valid
/// chDAG units.
/// </summary>
namespace Aurem.Nodes
{
    /// <summary>
    /// A node in Aleph represents an entity that is producing DAG units for a
    /// network of nodes.
    /// </summary>
    public class Node
    {
        private Ulid _id;
        // Each node has its on local copy of the chDAG in the network.
        private chDAG _chDAG;

        public Node(Ulid id, Network network)
        {
            _id = id;
            // We first register the node to the network, and then we create a
            // chDAG associated to this network.
            network.Add(this);
            _chDAG = new(network);
        }

        public void CreateUnit(byte[] data)
        {
            _chDAG.Add(new Unit(data));
        }
    }
}
