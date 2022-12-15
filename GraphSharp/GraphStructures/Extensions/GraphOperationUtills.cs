using System;
using System.Collections.Generic;
using GraphSharp.Common;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public RentedArray<int> CountDegrees(IEnumerable<TEdge> edges){
        var result = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId+1);
        foreach(var e in edges){
            result[e.SourceId]++;
            result[e.TargetId]++;
        }
        return result;
    }
    /// <summary>
    /// Apply Kruskal algorithm on set edges.
    /// </summary>
    /// <param name="edges">Spanning tree edges</param>
    /// <param name="maxDegree">Maximal degree that limits tree building</param>
    public KruskalForest<TEdge> KruskalAlgorithm(IEnumerable<TEdge> edges, Func<TNode,int> maxDegree)
    {
        using UnionFind unionFind = new(Nodes.MaxNodeId + 1);
        using var degree = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId+1);
        var outputEdges = new List<TEdge>();
        foreach (var n in Nodes)
            unionFind.MakeSet(n.Id);
        int sourceId = 0, targetId = 0;
        foreach (var edge in edges)
        {
            sourceId = edge.SourceId;
            targetId = edge.TargetId;
            if (unionFind.FindSet(sourceId) == unionFind.FindSet(targetId))
                continue;
            if(degree[sourceId]+1>maxDegree(Nodes[sourceId]) || degree[targetId]+1>maxDegree(Nodes[targetId]))
                continue;
            outputEdges.Add(edge);
            degree[sourceId]++;
            degree[targetId]++;
            unionFind.UnionSet(sourceId, targetId);
        }
        return new(unionFind,degree,outputEdges);
    }
}