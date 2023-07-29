using System.Linq;

namespace GraphSharp.Graphs;

/// <summary>
/// Base class for coloring algorithms
/// </summary>
public abstract class ColoringAlgorithmBase<TNode,TEdge> : Algorithms.ImmutableAlgorithmBase<TNode,TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<inhericdoc/>
    protected ColoringAlgorithmBase(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges) : base(nodes, edges)
    {
    }
    /// <summary>
    /// Performs graph nodes coloring
    /// </summary>
    /// <returns>Coloring result</returns>
    public abstract ColoringResult Color();
    /// <returns>Id of available color</returns>
    protected int GetAvailableColor(int nodeId, RentedArray<int> colors)
    {
        var neighborsColors = Edges.Neighbors(nodeId).Select(x=>colors[x]).ToList();
        return Enumerable.Range(1,neighborsColors.Max()+1).Except(neighborsColors).First();
    }
    /// <summary>
    /// Assigns first available color
    /// </summary>
    protected void AssignColor(int nodeId, RentedArray<int> colors)
    {
        colors[nodeId] = GetAvailableColor(nodeId, colors);
    }
    /// <returns>Amount of neighbors that have different colors</returns>
    protected int DegreeOfSaturation(int nodeId, RentedArray<int> colors)
    {
        return Edges.Neighbors(nodeId).DistinctBy(x => colors[x]).Count();
    }
}
