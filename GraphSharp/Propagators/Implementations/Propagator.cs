using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Single threaded <see cref="PropagatorBase{TNode,TEdge}"/> implementation.<br/>
/// <inheritdoc />
/// </summary>
public class Propagator<TNode, TEdge> : PropagatorBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <inheritdoc/>
    public Propagator(IVisitor<TNode, TEdge> visitor, IImmutableGraph<TNode, TEdge> graph) : base(visitor, graph)
    {
    }
    ///<inheritdoc/>
    protected override void PropagateNodes()
    {
        var nodes = Graph.Nodes;
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
                Visitor.Visit(nodes[nodeId]);
        };
    }

}