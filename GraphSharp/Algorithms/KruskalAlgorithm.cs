using System;
using System.Collections.Generic;
namespace GraphSharp.Graphs;

/// <summary>
/// Basic kruskal algorithm implementation 
/// </summary>
/// <typeparam name="TNode"></typeparam>
/// <typeparam name="TEdge"></typeparam>
public class KruskalAlgorithm<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <param name="nodes">Graph nodes</param>
    /// <param name="edges">Graph edges</param>
    public KruskalAlgorithm(IImmutableNodeSource<TNode> nodes, IEnumerable<TEdge> edges)
    {
        Nodes = nodes;
        Edges = edges;
    }
    /// <summary>
    /// Graph nodes
    /// </summary>
    public IImmutableNodeSource<TNode> Nodes { get; }
    /// <summary>
    /// Graph edges
    /// </summary>
    public IEnumerable<TEdge> Edges { get; }
    /// <summary>
    /// Apply Kruskal algorithm on set edges.
    /// </summary>
    /// <param name="maxDegree">Max node degree limiter</param>
    /// <returns>Kruskal forest</returns>
    public KruskalForest<TEdge> Find(Func<TNode, int> maxDegree)
    {
        using UnionFind unionFind = new(Nodes.MaxNodeId + 1);
        using var degree = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);
        var outputEdges = new List<TEdge>();
        foreach (var n in Nodes)
            unionFind.MakeSet(n.Id);
        int sourceId = 0, targetId = 0;
        foreach (var edge in Edges)
        {
            sourceId = edge.SourceId;
            targetId = edge.TargetId;
            if (unionFind.FindSet(sourceId) == unionFind.FindSet(targetId))
                continue;
            if (degree[sourceId] + 1 > maxDegree(Nodes[sourceId]) || degree[targetId] + 1 > maxDegree(Nodes[targetId]))
                continue;
            outputEdges.Add(edge);
            degree[sourceId]++;
            degree[targetId]++;
            unionFind.UnionSet(sourceId, targetId);
        }
        return new(unionFind, degree, outputEdges);
    }
}
