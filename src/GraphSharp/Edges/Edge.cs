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
        public int CompareTo(IEdge other) => Node.CompareTo(other.Node);
        public override string ToString()
        {
            return $"Child to node {Node.Id}";
        }
    }
}