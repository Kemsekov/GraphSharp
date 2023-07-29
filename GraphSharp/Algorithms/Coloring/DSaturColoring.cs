using System.Linq;
namespace GraphSharp.Graphs;

/// <summary>
/// Slightly different implementation of DSatur coloring algorithm.<br/>
/// A lot better than greedy algorithm and just about a half of it's speed.
/// </summary>
public class DSaturColoring<TNode, TEdge> : ColoringAlgorithmBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<inhericdoc/>
    public DSaturColoring(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges) : base(nodes, edges)
    {
    }
    ///<inhericdoc/>
    public override ColoringResult Color()
    {
        var colors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);
        var order = Nodes.OrderBy(x => -Edges.Neighbors(x.Id).Count());

        int coloredNodesCount = 0;
        foreach (var n in order)
        {
            if (colors[n.Id] != 0) continue;
            var toColor = n.Id;
            while (coloredNodesCount != Nodes.Count())
            {
                AssignColor(toColor, colors);
                coloredNodesCount++;
                var neighbors =
                    Edges.Neighbors(toColor)
                    .Where(x => colors[x] == 0)
                    .ToList();
                if (neighbors.Count == 0)
                    break;
                toColor = neighbors.MaxBy(x => DegreeOfSaturation(x, colors));
            }
        }
        return new(colors);
    }
}
