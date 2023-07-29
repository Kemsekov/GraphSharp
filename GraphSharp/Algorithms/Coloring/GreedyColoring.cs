using System.Linq;
namespace GraphSharp.Graphs;

/// <summary>
/// Linear-time greedy graph nodes coloring algorithm.<br/>
/// </summary>
public class GreedyColoring<TNode, TEdge> : ColoringAlgorithmBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<inhericdoc/>
    public GreedyColoring(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges) : base(nodes, edges)
    {
    }
    ///<inhericdoc/>
    public override ColoringResult Color()
    {
        var order = Nodes.OrderBy(x => -Edges.Neighbors(x.Id).Count());
        // var order = Nodes;

        var colors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);

        foreach (var n in order)
        {
            AssignColor(n.Id, colors);
        }

        return new(colors);
    }
}
