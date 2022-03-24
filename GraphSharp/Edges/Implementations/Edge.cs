using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Basic <see cref="IEdge{INode}"/> implementation
    /// </summary>
    public class Edge : EdgeBase<INode>
    {
        public Edge(INode parent, INode node) : base(parent, node)
        {
        }
        public override string ToString()
        {
            return $"Edge of node {Node.Id}";
        }
    }
}