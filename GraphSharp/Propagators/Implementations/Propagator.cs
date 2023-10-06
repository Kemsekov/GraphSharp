using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Single threaded <see cref="PropagatorBase{TEdge}"/> implementation.<br/>
/// <inheritdoc />
/// </summary>
public class Propagator<TEdge> : PropagatorBase<TEdge>
where TEdge : IEdge
{
    /// <inheritdoc/>
    public Propagator(IImmutableEdgeSource<TEdge> edges,IVisitor<TEdge> visitor, int maxNodeId = -1) : base(edges, visitor,maxNodeId)
    {
    }
    ///<inheritdoc/>
    protected override void PropagateNodes()
    {
        byte state = 0;
        for (int nodeId = 0; nodeId < NodeStates.Length; ++nodeId)
        {
            state = NodeStates.GetState(nodeId);
            if (ByteStatesHandler.IsInState(UsedNodeStates.ToVisit,state))
                PropagateNode(nodeId,state);
        };
        for (int nodeId = 0; nodeId < NodeStates.Length; ++nodeId)
        {
            if (NodeStates.IsInState(UsedNodeStates.Visited,nodeId))
                Visitor.Visit(nodeId);
        };
    }

}