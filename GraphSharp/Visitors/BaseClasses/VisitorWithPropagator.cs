using System;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
namespace GraphSharp.Visitors;

/// <summary>
/// Base implementation of <see cref="IVisitor{TEdge}"/> and proxy of <see cref="IPropagator{TEdge}"/> in one instance.
/// </summary>
public abstract class VisitorWithPropagator<TEdge> : VisitorBase<TEdge>, IPropagator<TEdge>
where TEdge : IEdge
{
    /// <summary>
    /// <see cref="PropagatorBase{TEdge}"/> implementation that used for this proxy class
    /// </summary>
    public abstract PropagatorBase<TEdge> Propagator { get; }
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
    public void Reset(IImmutableEdgeSource<TEdge> edges, IVisitor<TEdge> visitor, int maxNodeId = -1)
    {
        Propagator.Reset(edges,visitor,maxNodeId);
    }
}
