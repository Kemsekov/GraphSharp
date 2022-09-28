using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds a spanning tree using Kruskal algorithm
    /// </summary>
    /// <param name="getWeight">When null spanning tree is computed by sorting edges by weights. If you need to change this behavior specify this delegate, so edges will be sorted in different order.</param>
    /// <returns>List of edges that form a minimal spanning tree</returns>
    public IList<TEdge> FindSpanningTreeKruskal(Func<TEdge, float>? getWeight = null)
    {
        getWeight ??= e => e.Weight;
        var edges = Edges.OrderBy(x => getWeight(x));
        var result = new List<TEdge>();
        KruskalAlgorithm(edges, result);
        return result;
    }
    /// <summary>
    /// Apply Kruskal algorithm on set edges.
    /// </summary>
    /// <param name="edges">Spanning tree edges</param>
    void KruskalAlgorithm(IEnumerable<TEdge> edges, IList<TEdge> outputEdges)
    {
        using UnionFind unionFind = new(Nodes.MaxNodeId + 1);
        foreach (var n in Nodes)
            unionFind.MakeSet(n.Id);
        int SourceId = 0, targetId = 0;
        foreach (var edge in edges)
        {
            SourceId = edge.SourceId;
            targetId = edge.TargetId;
            if (unionFind.FindSet(SourceId) == unionFind.FindSet(targetId))
                continue;
            outputEdges.Add(edge);
            unionFind.UnionSet(SourceId, targetId);
        }
    }
}