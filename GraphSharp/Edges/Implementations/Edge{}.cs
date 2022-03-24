using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Edge with some value alongside with it.
    /// </summary>
    /// <typeparam name="TWeight"></typeparam>
    public class Edge<TWeight> : Edge
    {
        public Edge(Node parent, Node node, TWeight value) : base(parent, node)
        {
            Value = value;
        }
        public TWeight Value { get; set; }
    }
}