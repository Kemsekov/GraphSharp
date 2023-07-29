using System;
using System.Collections;
using System.Collections.Generic;

namespace GraphSharp.Graphs;

/// <summary>
/// Result of independent set finders algorithms
/// </summary>
public class IndependentSetResult<TNode> : IEnumerable<TNode>, IDisposable
{
    const byte Added = 1;
    RentedArray<byte> NodeState { get; }
    /// <summary>
    /// Nodes in given independent set
    /// </summary>
    public IEnumerable<TNode> Nodes { get; }
    /// <summary>
    /// </summary>
    public IndependentSetResult(RentedArray<byte> nodeState, IEnumerable<TNode> nodes)
    {
        NodeState = nodeState;
        Nodes = nodes;
    }
    /// <summary>
    /// Determine whatever given node is in given independent set
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public bool IsAdded(int nodeId) => (NodeState[nodeId] & Added) == Added;
    /// <inheritdoc/>

    public IEnumerator<TNode> GetEnumerator()
    {
        return Nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Nodes).GetEnumerator();
    }
    /// <inheritdoc/>
    public void Dispose()
    {
        NodeState.Dispose();
    }
}
