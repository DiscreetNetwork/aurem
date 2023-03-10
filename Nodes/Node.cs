using NUlid;
using Aurem.chDAGs;
using Aurem.Randomness;
using Aurem.Units;
using Aurem.Networking;

/// <summary>
/// Node represents a Unit creator; an entity responsible for creating valid
/// chDAG units.
/// </summary>
namespace Aurem.Nodes
{
    /// <summary>
    /// A node represents an entity that is producing DAG units for a
    /// network of nodes.
    /// </summary>
    public class Node
    {
        public Ulid Id;
        // Each node has its on local copy of the chDAG in the network.
        public chDAG _chDAG { get; }
        private SecretKey _sk;

        public Node(Ulid id, Network network, SecretKey sk)
        {
            Id = id;
            // We first register the node to the network, and then we create a
            // chDAG associated to this network.
            network.Add(this);
            _chDAG = new(network, this);
            _sk = sk;
        }

        /// <summary>
        /// GetChDAG returns the chDAG to which the node is registered to.
        /// </summary>
        public chDAG GetChDAG()
        {
            return _chDAG;
        }

        /// <summary>
        /// CreateUnit creates a new unit for this node's chDAG, which contains
        /// some data.
        /// </summary>
        public void CreateUnit(byte[] data)
        {
            _chDAG.Add(new Unit(Id, data));
        }
    }
}
