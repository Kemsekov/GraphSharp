using System;
using System.Linq;
namespace GraphSharp.Graphs;

/// <summary>
/// Recursive largest first algorithm. The most efficient in colors used algorithm,
/// but the slowest one.
/// </summary>
public class RLFColoring<TNode, TEdge> : ColoringAlgorithmBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<inhericdoc/>
    public RLFColoring(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges) : base(nodes, edges)
    {
    }
    ///<inhericdoc/>
    public override ColoringResult Color()
    {
        int coloredNodesCount = 0;
        int colorIndex = 1;
        var colors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId+1);
        while (coloredNodesCount != Nodes.Count())
        {
            var S = FindMaximalIndependentSet(colors,x => colors[x.Id]==0);
            var count = S.Count();
            coloredNodesCount += count;
            foreach (var node in S)
                colors[node.Id] = colorIndex;
            colorIndex++;
        }
        return new(colors);
    }

    private IndependentSetResult<TNode> FindMaximalIndependentSet(RentedArray<int> colors, Predicate<TNode> condition)
    {
        using var alg = new BallardMyerIndependentSet<TNode,TEdge>(Nodes,Edges,x => colors[x.Id]==0);
        return alg.Find();
    }
}
