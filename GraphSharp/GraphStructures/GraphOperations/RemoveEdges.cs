using System;
using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Removes all edges that satisfies predicate.
    /// </summary>
    public GraphOperation<TNode, TEdge> RemoveEdges(Predicate<TEdge> toRemove)
    {
        var edgesToRemove =
            Edges.Where(x => toRemove(x))
            .Select(x => (x.SourceId, x.TargetId))
            .ToArray();

        foreach (var e in edgesToRemove)
            Edges.Remove(e.Item1, e.Item2);

        return this;
    }
}