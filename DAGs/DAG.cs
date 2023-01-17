using System.Collections;
using DotNetGraph;
using DotNetGraph.Node;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;

/// <summary>
/// DAG provides methods for handling Directed Acyclic Graphs.
/// <summary>
namespace Aurem.DAGs
{
    public class DAG<NodeType> : ISet<NodeType>
    {
        // The number of required parents when creating a new unit in the DAG.
        // The minimum should be equal to (N-(N-1)/3), derived from the formula
        // N = 3f+1, where f is the number of Byzantine nodes and N is the total
        // number of node creators.
        private int _numParents;
        private HashSet<NodeType> _units;

        public DAG()
        {
            _numParents = 5;
            _units = new HashSet<NodeType>();
        }

        public bool Add(NodeType unit) {
            return _units.Add(unit); // TODO
        }

        public bool Add(IEnumerable<NodeType> units) {
            return true; // TODO
        }

        /// <summary>
        /// Creates a PNG of the DAG.
        /// </summary>
        public void Save() {
            var graph = new DotGraph("DAG");

            var myNode = new DotNode("Node")
            {
                Shape = DotNodeShape.Ellipse,
                Label = "My node!",
                FillColor = System.Drawing.Color.Coral,
                FontColor = System.Drawing.Color.Black,
                Style = DotNodeStyle.Dotted,
                Width = 0.5f,
                Height = 0.5f,
                PenWidth = 1.5f
            };

            // Add the node to the graph
            graph.Elements.Add(myNode);

            // Create an edge with identifiers
            // var myEdge = new DotEdge("myNode1", "myNode2");

            // Create an edge with nodes and attributes
            var myEdge = new DotEdge(myNode, myNode)
            {
                ArrowHead = DotEdgeArrowType.Box,
                ArrowTail = DotEdgeArrowType.Diamond,
                Color = System.Drawing.Color.Red,
                FontColor = System.Drawing.Color.Black,
                Label = "My edge!",
                Style = DotEdgeStyle.Dashed,
                PenWidth = 1.5f
            };

            // Add the edge to the graph
            graph.Elements.Add(myEdge);

            var dot = graph.Compile(true);
            File.WriteAllText("graph.dot", dot);
            // Run it yourself in a terminal...
            // Process.Start("dot", "-Tpng -o graph.png graph.dot").WaitForExit();
        }

        // Sigh...
        public int Count => _units.Count;
        public IEnumerator<NodeType> GetEnumerator() => _units.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _units.GetEnumerator();
        public bool IsReadOnly => false;
        public bool Remove(NodeType unit) => true; // Do we even need this? In case yes: TODO
        public void CopyTo(NodeType[] units, int index) => _units.CopyTo(units, index);
        public bool Contains(NodeType item) =>  _units.Contains(item);
        public void Clear() => _units.Clear();
        public void UnionWith(IEnumerable<NodeType> other) => this.Add(other);
        void ICollection<NodeType>.Add(NodeType item) => this.Add(item);
        public void SymmetricExceptWith(ICollection<NodeType> units) { }
        public void SymmetricExceptWith(IEnumerable<NodeType> units) { }
        public bool SetEquals(IEnumerable<NodeType> units) => _units.SetEquals(units);
        public bool Overlaps(IEnumerable<NodeType> units) => _units.Overlaps(units);
        public bool IsSupersetOf(IEnumerable<NodeType> units) => _units.IsSupersetOf(units);
        public bool IsSubsetOf(IEnumerable<NodeType> units) => _units.IsSubsetOf(units);
        public bool IsProperSubsetOf(IEnumerable<NodeType> units) => _units.IsProperSubsetOf(units);
        public bool IsProperSupersetOf(IEnumerable<NodeType> units) => _units.IsProperSupersetOf(units);
        public void IntersectWith(DAG<NodeType> dag) => this._units.IntersectWith(dag._units);
        public void IntersectWith(ICollection<NodeType> unit) {  }
        public void IntersectWith(IEnumerable<NodeType> unit) {  }
        public void ExceptWith(IEnumerable<NodeType> unit) {  }
    }
}
