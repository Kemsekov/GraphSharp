using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
    /// <summary>
    /// Finds a spanning tree using Kruskal algorithm
    /// </summary>
    /// <param name="getWeight">When null spanning tree is computed by sorting edges by weights. If you need to change this behavior specify this delegate, so edges will be sorted in different order.</param>
    /// <returns>List of edges that form a minimal spanning tree</returns>
    public IList<TEdge> FindSpanningTree(Func<TEdge, float>? getWeight = null)
    {
        getWeight ??= e => e.Weight;
        var Edges = _structureBase.Edges;
        var Configuration = _structureBase.Configuration;
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
        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        var Configuration = _structureBase.Configuration;
        UnionFind unionFind = new(Nodes.MaxNodeId + 1);
        foreach (var n in Nodes)
            unionFind.MakeSet(n.Id);
        int sourceId = 0, targetId = 0;
        foreach (var edge in edges)
        {
            sourceId = edge.Source.Id;
            targetId = edge.Target.Id;
            if (unionFind.FindSet(sourceId) == unionFind.FindSet(targetId))
                continue;
            outputEdges.Add(edge);
            unionFind.UnionSet(sourceId, targetId);
        }
    }
}