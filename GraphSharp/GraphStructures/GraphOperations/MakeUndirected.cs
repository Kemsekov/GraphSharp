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
    /// Makes every connection between two nodes bidirectional, producing undirected graph.
    /// </summary>
    public GraphOperation<TNode, TEdge> MakeUndirected(Action<TEdge>? onCreatedEdge = null)
    {
        onCreatedEdge ??= (edge) => { };
        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        var Configuration = _structureBase.Configuration;
        foreach (var source in Nodes)
        {
            var edges = Edges[source.Id];
            foreach (var edge in edges)
            {
                if (Edges.TryGetEdge(edge.Target.Id, edge.Source.Id, out var _)) continue;
                var newEdge = Configuration.CreateEdge(edge.Target, edge.Source);
                onCreatedEdge(newEdge);
                Edges.Add(newEdge);
            }
        };
        return this;
    }
}