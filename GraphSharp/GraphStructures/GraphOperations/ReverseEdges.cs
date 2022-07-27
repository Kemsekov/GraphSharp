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
    /// Reverse every edge connection ==> like swap(edge.Source,edge.Target)
    /// </summary>
    public GraphOperation<TNode, TEdge> ReverseEdges()
    {
        var Configuration = _structureBase.Configuration;
        var Edges = _structureBase.Edges;

        var toSwap =
            Edges.Where(x => !Edges.TryGetEdge(x.Target.Id, x.Source.Id, out var _))
            .Select(x => (x.Source.Id, x.Target.Id))
            .ToArray();

        foreach (var e in toSwap)
        {
            var edge = Edges[e.Item1, e.Item2];
            Edges.Remove(e.Item1, e.Item2);
            var tmp = edge.Source;
            edge.Source = edge.Target;
            edge.Target = tmp;
            Edges.Add(edge);
        }

        return this;
    }
}