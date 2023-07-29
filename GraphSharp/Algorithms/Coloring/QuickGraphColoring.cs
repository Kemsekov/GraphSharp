using GraphSharp.Adapters;
using QuikGraph.Algorithms.VertexColoring;
namespace GraphSharp.Graphs;

/// <summary>
/// QuikGraph's coloring algorithm
/// </summary>
public class QuickGraphColoring<TNode, TEdge> : ColoringAlgorithmBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<inhericdoc/>
    public QuickGraphColoring(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges) : base(nodes, edges)
    {
    }
    ///<inhericdoc/>
    public override ColoringResult Color()
    {

        var quikGraph = new ToQuikGraphAdapter<TNode, TEdge>(Nodes, Edges);
        var coloring = new VertexColoringAlgorithm<int, EdgeAdapter<TEdge>>(quikGraph);
        coloring.Compute();
        var result = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);
        for (int i = 0; i < result.Length; i++)
        {
            if (coloring.Colors[i] is int color)
                result[i] = color;
        }
        return new(result);
    }
}
