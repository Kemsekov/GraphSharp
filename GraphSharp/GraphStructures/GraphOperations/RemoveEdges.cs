using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
    /// <summary>
    /// Removes all edges that satisfies predicate.
    /// </summary>
    public GraphOperation<TNode, TEdge> RemoveEdges(Predicate<TEdge> toRemove)
    {
        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        var Configuration = _structureBase.Configuration;
        var edgesToRemove =
            Edges.Where(x => toRemove(x))
            .Select(x => (x.Source.Id, x.Target.Id))
            .ToArray();

        foreach (var e in edgesToRemove)
            Edges.Remove(e.Item1, e.Item2);

        return this;
    }
}