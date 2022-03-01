using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Basic <see cref="IEdge{INode}"/> implementation
    /// </summary>
    public class Edge : EdgeBase<INode>
    {
        public Edge(INode node) : base(node)
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