using Aurem.DAGs;

/// <summary>
/// chDAG is a specific case of a DAG, where the storage of units is bound by
/// restrictions that facilitate a network of nodes to achieve consensus
/// <summary>
namespace Aurem.chDAGs
{
    public class chDAG<NodeType> : DAG<NodeType>
    {
        /// <summary>
        /// GetParents needs to return the minimum required number of parents
        /// to avoid a Byzantine attack.
        /// <summary>
        public List<NodeType> GetParents()
        {
            // TODO
            // Task.Delay(1000);
            return new List<NodeType>();
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
    }
}
