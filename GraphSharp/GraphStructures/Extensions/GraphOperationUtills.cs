using System;
using System.Collections.Generic;
using System.Linq;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Creates random automorphism of graph, producing two isomorphic graphs
    /// </summary>
    /// <returns>new graph that is isomorphic to input graph and mapping of original nodes to new graph</returns>
    public (Graph isomorphic, Dictionary<int, int> mapping) CreateRandomAutomorphism()
    {
        var sourceNodes = Nodes.Select(n=>n.Id).ToArray();
        var mapped = sourceNodes.OrderBy(i=>Random.Shared.Next()).ToArray();
        var mapping = sourceNodes.Zip(mapped).ToDictionary(k=>k.First,k=>k.Second);
        return (CreateAutomorphism(mapping),mapping);
    }
    /// <summary>
    /// Creates automorphism of graph, producing two isomorphic graphs
    /// </summary>
    /// <param name="mapping"></param>
    /// <returns>new graph that is isomorphic to input graph and mapping of original nodes to new graph</returns>
    public Graph CreateAutomorphism(IDictionary<int, int> mapping)
    {
        var sourceNodes = Nodes.Select(n=>n.Id).ToArray();

        var isomorphic = new Graph();
        foreach(var n in Nodes){
            isomorphic.Nodes.Add(new Node(mapping[n.Id]));
        }
        foreach(var e in Edges){
            var source = mapping[e.SourceId];
            var target = mapping[e.TargetId];
            isomorphic.Edges.Add(new Edge(source,target));
        }
        return isomorphic;
    }
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