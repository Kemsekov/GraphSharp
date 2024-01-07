using System.Linq;
using GraphSharp.Adapters;
using QuikGraph.Algorithms.Ranking;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Uses quik graph implementation to find page rank of current graph.
    /// </summary>
    /// <param name="damping">
    /// Damping factor of page rank. 
    /// In other words how likely that visitor from node A will go to edge A->B. 
    /// So <see langword="1-damping"/> equals to probability that visitor from node A will jump
    /// to any other random node C, that can be adjacent to A, or not.
    /// </param>
    /// <param name="tolerance">
    /// What precision needs to be achieved.
    /// </param>
    public PageRankAlgorithm<int, EdgeAdapter<TEdge>> PageRank(double damping = 0.85, double tolerance = 0.001)
    {
        var quikGraph = StructureBase.Converter.ToQuikGraph();
        var pageRank = new QuikGraph.Algorithms.Ranking.PageRankAlgorithm<int,EdgeAdapter<TEdge>>(quikGraph);
        pageRank.Damping = damping;
        pageRank.Tolerance = tolerance;
        pageRank.Compute();
        return pageRank;
    }
}