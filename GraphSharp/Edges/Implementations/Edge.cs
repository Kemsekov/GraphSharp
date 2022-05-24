using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Basic <see cref="IEdge{INode}"/> implementation
    /// </summary>
    public class Edge : EdgeBase<INode>
    {
        public Edge(INode source, INode target) : base(source, target)
        {
        }
        public override string ToString()
        {
            return $"Edge of node {Target.Id}";
        }
    }
}