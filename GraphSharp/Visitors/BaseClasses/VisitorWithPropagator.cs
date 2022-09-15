using System;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
namespace GraphSharp.Visitors;

/// <summary>
/// Base implementation of <see cref="IVisitor{,}"/> and proxy of <see cref="IPropagator{,}"/> in one instance.
/// </summary>
public abstract class VisitorWithPropagator<TNode, TEdge> : VisitorBase<TNode, TEdge>, IPropagator<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// <see cref="PropagatorBase{,}"/> implementation that used for this proxy class
    /// </summary>
    public abstract PropagatorBase<TNode, TEdge> Propagator { get; }
    public void Propagate()
    {
        Propagator.Propagate();
    }

    public void SetPosition(params int[] nodeIndices)
    {
        Propagator.SetPosition(nodeIndices);
    }

    public void Reset(IGraph<TNode, TEdge> graph, IVisitor<TNode,TEdge> visitor)
    {
        Propagator.Reset(graph,visitor);
    }

    public bool IsNodeInState(int nodeId, byte state)
    {
        return Propagator.IsNodeInState(nodeId, state);
    }

    public void AddNodeState(int nodeId, byte state)
    {
        Propagator.AddNodeState(nodeId, state);
    }

    public void RemoveNodeState(int nodeId, byte state)
    {
        Propagator.RemoveNodeState(nodeId, state);
    }

    public byte GetNodeStates(int nodeId)
    {
        return Propagator.GetNodeStates(nodeId);
    }

    public void ClearNodeStates(int nodeId)
    {
        Propagator.ClearNodeStates(nodeId);
    }
}
