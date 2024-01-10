using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Randomly makes every connection between two nodes directed.
    /// </summary>
    public GraphOperation<TNode, TEdge> MakeDirected()
    {
        Edges.MakeDirected();
        return this;
    }
}