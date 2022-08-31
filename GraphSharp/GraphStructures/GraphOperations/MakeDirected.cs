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
        foreach (var n in Nodes)
        {
            foreach (var e in Edges.OutEdges(n.Id).ToList())
            {
                Edges.Remove(e.TargetId, e.SourceId);
            }
        }
        return this;
    }
}