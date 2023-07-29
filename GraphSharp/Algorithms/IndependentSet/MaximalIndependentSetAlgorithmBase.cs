using System;

namespace GraphSharp.Graphs;

/// <summary>
/// Base class for maximal independent set algorithms
/// </summary>
public abstract class IndependentSetAlgorithmBase<TNode, TEdge> : Algorithms.ImmutableAlgorithmBase<TNode, TEdge>, IDisposable
where TNode : INode
where TEdge : IEdge
{
#pragma warning disable
    protected const byte Added = 1;
    protected const byte AroundAdded = 2;
    protected const byte Forbidden = 4;
    protected RentedArray<byte> nodeState;
    protected RentedArray<int> freeNeighbors;
    protected RentedArray<int> countOfForbiddenNeighbors;
    protected RentedArray<int> countOfColoredNeighbors;
#pragma warning enable
    ///<inheritdoc/>
    protected IndependentSetAlgorithmBase(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges) : base(nodes, edges)
    {
        nodeState = ArrayPoolStorage.RentArray<byte>(Nodes.MaxNodeId + 1);
        freeNeighbors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);
        countOfForbiddenNeighbors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);
        countOfColoredNeighbors = ArrayPoolStorage.RentArray<int>(Nodes.MaxNodeId + 1);
    }
#pragma warning disable
    protected bool IsAdded(int nodeId) => (nodeState[nodeId] & Added) == Added;
    protected bool IsForbidden(int nodeId) => (nodeState[nodeId] & Forbidden) == Forbidden;
    protected bool IsAroundAdded(int nodeId) => (nodeState[nodeId] & AroundAdded) == AroundAdded;
    protected int CountOfUncoloredNeighbors(int nodeId)
    {
        int result = 0;
        foreach (var n in Edges.Neighbors(nodeId))
            if (nodeState[n] == 0) result++;
        return result;
    }
#pragma warning enable

    /// <summary>
    /// Finds maximal independent set
    /// </summary>
    public abstract IndependentSetResult<TNode> Find();

    ///<inheritdoc/>
    void IDisposable.Dispose()
    {
        nodeState.Dispose();
        freeNeighbors.Dispose();
        countOfForbiddenNeighbors.Dispose();
        countOfColoredNeighbors.Dispose();
    }
}
