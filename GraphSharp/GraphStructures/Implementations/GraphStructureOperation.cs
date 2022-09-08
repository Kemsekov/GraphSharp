using GraphSharp.Common;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

/// <summary>
/// Contains graph algorithms.
/// </summary>
public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    IGraph<TNode, TEdge> StructureBase{get;}
    INodeSource<TNode> Nodes => StructureBase.Nodes;
    IEdgeSource<TEdge> Edges => StructureBase.Edges;
    IGraphConfiguration<TNode, TEdge> Configuration => StructureBase.Configuration;
    ObjectPool<Propagator<TNode, TEdge>> PropagatorPool;
    ObjectPool<ParallelPropagator<TNode, TEdge>> ParallelPropagatorPool;
    public GraphOperation(IGraph<TNode, TEdge> structureBase)
    {
        StructureBase = structureBase;
        var tmpVisitor = new ActionVisitor<TNode, TEdge>();
        PropagatorPool = new(() => new Propagator<TNode, TEdge>(tmpVisitor, StructureBase));
        ParallelPropagatorPool = new(() => new ParallelPropagator<TNode, TEdge>(tmpVisitor, StructureBase));
    }
    /// <summary>
    /// Get propagator from pool
    /// </summary>
    public Propagator<TNode, TEdge> GetPropagator(IVisitor<TNode, TEdge> visitor)
    {
        var p = PropagatorPool.Get();
        p.Reset(StructureBase, visitor);
        return p;
    }
    /// <summary>
    /// Get parallel propagator from pool
    /// </summary>
    public ParallelPropagator<TNode, TEdge> GetParallelPropagator(IVisitor<TNode, TEdge> visitor)
    {
        var p = ParallelPropagatorPool.Get();
        p.Reset(StructureBase, visitor);
        return p;
    }
    /// <summary>
    /// Returns propagator to pool
    /// </summary>
    public void ReturnPropagator(IPropagator<TNode, TEdge> propagator){
        if(propagator is ParallelPropagator<TNode,TEdge> p1)
            ParallelPropagatorPool.Return(p1);
        if(propagator is Propagator<TNode,TEdge> p2)
            PropagatorPool.Return(p2);
    }
    
}