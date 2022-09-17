using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Single threaded <see cref="PropagatorBase{,}"/> implementation.<br/>
/// <inheritdoc />
/// </summary>
public class Propagator<TNode, TEdge> : PropagatorBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <inheritdoc cref="Propagator{,}" />
    public Propagator(IVisitor<TNode, TEdge> visitor, IGraph<TNode, TEdge> graph) : base(visitor, graph)
    {
    }
    protected override void PropagateNodes()
    {
        var nodes = Graph.Nodes;
        for (int nodeId = 0; nodeId < NodeStates.Length; ++nodeId)
        {
            if (NodeStates.IsInState(UsedNodeStates.ToVisit,nodeId))
                PropagateNode(nodeId);
        };
        for (int nodeId = 0; nodeId < NodeStates.Length; ++nodeId)
        {
            if (NodeStates.IsInState(UsedNodeStates.Visited,nodeId))
                Visitor.Visit(nodes[nodeId]);
        };
    }

}