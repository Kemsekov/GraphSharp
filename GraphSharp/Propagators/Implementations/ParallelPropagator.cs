using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Concurrent <see cref="PropagatorBase{TNode,TEdge}"/> implementation.<br/> 
/// Every <see cref="IVisitor{TNode,TEdge}"/> that accompany this propagator must be implemented
/// as thread-safe one.
/// <inheritdoc />
/// </summary>
public class ParallelPropagator<TNode, TEdge> : PropagatorBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <inheritdoc cref="ParallelPropagator{TNode,TEdge}" />
    public ParallelPropagator(IVisitor<TNode, TEdge> visitor, IImmutableGraph<TNode, TEdge> graph) : base(visitor, graph)
    {
    }
    ///<inheritdoc/>
    protected override void PropagateNodes()
    {
        var nodes = Graph.Nodes;
        Parallel.For(0, NodeStates.Length, nodeId =>
        {
            var state = NodeStates.GetState(nodeId);
            if (ByteStatesHandler.IsInState(UsedNodeStates.ToVisit,state))
                PropagateNode(nodeId,state);
        });
        Parallel.For(0, NodeStates.Length, nodeId =>
        {
            if (NodeStates.IsInState(UsedNodeStates.Visited,nodeId))
                Visitor.Visit(nodes[nodeId]);
        });
    }
}