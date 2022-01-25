using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    public class Edge<T> : Edge
    {
        public Edge(INode node, T value) : base(node)
        {
            Value = value;
        }
        public T Value { get; protected set; }
    }
}