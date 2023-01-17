using NUlid;
using Aurem.chDAGs;
using Aurem.Units;

/// <summary>
/// Node represents a Unit creator; an entity responsible for creating valid
/// chDAG units.
/// </summary>
namespace Aurem.Nodes
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
}
