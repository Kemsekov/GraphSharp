using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Reverse every edge connection ==> like swap(edge.Source,edge.Target)
    /// </summary>
    public GraphOperation<TNode, TEdge> ReverseEdges()
    {
        var toSwap =
            Edges.Where(x => !Edges.TryGetEdge(x.TargetId, x.SourceId, out var _))
            .Select(x => (x.SourceId, x.TargetId))
            .ToArray();

        foreach (var e in toSwap)
        {
            var edge = Edges[e.Item1, e.Item2];
            Edges.Remove(e.Item1, e.Item2);
            var tmp = edge.SourceId;
            edge.SourceId = edge.TargetId;
            edge.TargetId = tmp;
            Edges.Add(edge);
        }

        return this;
    }
}