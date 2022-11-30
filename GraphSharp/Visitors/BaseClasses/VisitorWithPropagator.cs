using System;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
namespace GraphSharp.Visitors;

/// <summary>
/// Base implementation of <see cref="IVisitor{TNode,TEdge}"/> and proxy of <see cref="IPropagator{TNode,TEdge}"/> in one instance.
/// </summary>
public abstract class VisitorWithPropagator<TNode, TEdge> : VisitorBase<TNode, TEdge>, IPropagator<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// <see cref="PropagatorBase{TNode,TEdge}"/> implementation that used for this proxy class
    /// </summary>
    public abstract PropagatorBase<TNode, TEdge> Propagator { get; }
    /// <summary>
    /// Function that used to propagate any exploration algorithm
    /// </summary>
    public void Propagate()
    {
        Propagator.Propagate();
    }

    /// <summary>
    /// Changes current exploration position
    /// </summary>
    public void SetPosition(params int[] nodeIndices)
    {
        Propagator.SetPosition(nodeIndices);
    }
    /// <summary>
    /// Resets propagator and whole exploration algorithm with new graph and visitor
    /// </summary>
    public void Reset(IImmutableGraph<TNode, TEdge> graph, IVisitor<TNode,TEdge> visitor)
    {
        Propagator.Reset(graph,visitor);
    }
}
