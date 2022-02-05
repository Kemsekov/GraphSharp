using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Basic <see cref="IEdge"/> implementation
    /// </summary>
    public class Edge : IEdge
    {
        public Edge(INode node)
        {
            this.Node = node;
        }
        public INode Node{get;init;}
        public virtual int CompareTo(IEdge other) => Node.CompareTo(other.Node);
        public override string ToString()
        {
            return $"Edge of node {Node.Id}";
        }
    }
}