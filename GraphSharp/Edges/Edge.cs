using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
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
            return $"Edge to node {Node.Id}";
        }
    }
}