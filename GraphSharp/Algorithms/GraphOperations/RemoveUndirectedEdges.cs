using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Removes bidirected edges.
    /// </summary>
    public GraphOperation<TNode, TEdge> RemoveBidirectedEdges()
    {
        foreach (var n in Nodes)
        {
            var edges = Edges.OutEdges(n.Id).ToArray();
            foreach (var edge in edges)
            {
                if (Edges.Remove(edge.TargetId, edge.SourceId))
                {
                    Edges.Remove(edge);
                }
            }
        }
        return this;
    }
}