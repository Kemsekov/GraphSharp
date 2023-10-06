using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Concurrent <see cref="PropagatorBase{TEdge}"/> implementation.<br/> 
/// Every <see cref="IVisitor{TEdge}"/> that accompany this propagator must be implemented
/// as thread-safe one.
/// <inheritdoc />
/// </summary>
public class ParallelPropagator<TEdge> : PropagatorBase<TEdge>
where TEdge : IEdge
{
    /// <inheritdoc cref="ParallelPropagator{TEdge}" />
    public ParallelPropagator(IImmutableEdgeSource<TEdge> edges,IVisitor<TEdge> visitor, int maxNodeId = -1) : base(edges, visitor,maxNodeId)
    {
    }
    ///<inheritdoc/>
    protected override void PropagateNodes()
    {
        Parallel.For(0, NodeStates.Length, nodeId =>
        {
            var state = NodeStates.GetState(nodeId);
            if (ByteStatesHandler.IsInState(UsedNodeStates.ToVisit,state))
                PropagateNode(nodeId,state);
        });
        Parallel.For(0, NodeStates.Length, nodeId =>
        {
            if (NodeStates.IsInState(UsedNodeStates.Visited,nodeId))
                Visitor.Visit(nodeId);
        });
    }
}