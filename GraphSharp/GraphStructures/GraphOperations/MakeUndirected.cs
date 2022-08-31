using System;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Makes every connection between two nodes bidirectional by adding missing edges.
    /// </summary>
    public GraphOperation<TNode, TEdge> MakeUndirected(Action<TEdge>? onCreatedEdge = null)
    {
        onCreatedEdge ??= (edge) => { };
        foreach (var source in Nodes)
        {
            var edges = Edges.OutEdges(source.Id);
            foreach (var edge in edges)
            {
                if (Edges.TryGetEdge(edge.TargetId, edge.SourceId, out var _)) continue;
                var newEdge = Configuration.CreateEdge(Nodes[edge.TargetId], Nodes[edge.SourceId]);
                onCreatedEdge(newEdge);
                Edges.Add(newEdge);
            }
        };
        return this;
    }
}