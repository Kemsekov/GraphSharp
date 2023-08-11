using System;
using System.Collections.Generic;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Count node degrees from given edges
    /// </summary>
    public RentedArray<int> CountDegrees(IEnumerable<TEdge> edges)
    {
        var result = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);
        foreach (var e in edges)
        {
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
    public KruskalForest<TEdge> KruskalAlgorithm(IEnumerable<TEdge> edges, Func<TNode, int> maxDegree)
    {
       return new KruskalAlgorithm<TNode,TEdge>(Nodes,edges).Find(maxDegree);
    }
}