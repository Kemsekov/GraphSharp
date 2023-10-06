using System;
using GraphSharp.Common;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

/// <summary>
/// Contains graph algorithms.
/// </summary>
public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    IImmutableGraph<TNode, TEdge> StructureBase{get;}
    IImmutableNodeSource<TNode> Nodes => StructureBase.Nodes;
    IImmutableEdgeSource<TEdge> Edges => StructureBase.Edges;
    IGraphConfiguration<TNode, TEdge> Configuration => StructureBase.Configuration;
    ObjectPool<Propagator<TEdge>> PropagatorPool;
    ObjectPool<ParallelPropagator<TEdge>> ParallelPropagatorPool;
    ///<inheritdoc/>
    public ImmutableGraphOperation(IImmutableGraph<TNode, TEdge> structureBase)
    {
        StructureBase = structureBase;
        var tmpVisitor = new ActionVisitor<TNode, TEdge>();
        PropagatorPool = new(() => new Propagator<TEdge>(StructureBase.Edges,tmpVisitor,StructureBase.Nodes.MaxNodeId));
        ParallelPropagatorPool = new(() => new ParallelPropagator<TEdge>(StructureBase.Edges,tmpVisitor,StructureBase.Nodes.MaxNodeId));
    }
    /// <summary>
    /// Get propagator from pool
    /// </summary>
    public Propagator<TEdge> GetPropagator(IVisitor<TEdge> visitor)
    {
        var p = PropagatorPool.Get();
        p.Reset(StructureBase.Edges, visitor,StructureBase.Nodes.MaxNodeId);
        return p;
    }
    /// <summary>
    /// Get parallel propagator from pool
    /// </summary>
    public ParallelPropagator<TEdge> GetParallelPropagator(IVisitor<TEdge> visitor)
    {
        var p = ParallelPropagatorPool.Get();
        p.Reset(StructureBase.Edges, visitor,StructureBase.Nodes.MaxNodeId);
        return p;
    }
    /// <summary>
    /// Returns propagator to pool
    /// </summary>
    public void ReturnPropagator(IPropagator<TEdge> propagator){
        if(propagator is Propagator<TEdge> p1)
            PropagatorPool.Return(p1);
        if(propagator is ParallelPropagator<TEdge> p2)
            ParallelPropagatorPool.Return(p2);
    }
    
}