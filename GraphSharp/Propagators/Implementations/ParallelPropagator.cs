using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Concurrent <see cref="PropagatorBase{,}"/> implementation.<br/> 
/// Every <see cref="IVisitor{,}"/> that accompany this propagator must be implemented
/// as thread-safe one.
/// <inheritdoc />
/// </summary>
public class ParallelPropagator<TNode, TEdge> : PropagatorBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <inheritdoc cref="ParallelPropagator{,}" />
    public ParallelPropagator(IVisitor<TNode, TEdge> visitor, IGraph<TNode, TEdge> graph) : base(visitor, graph)
    {
    }
    protected override void PropagateNodes()
    {
        var nodes = Graph.Nodes;
        Parallel.For(0, NodeStates.Length, nodeId =>
        {
            if (NodeStates.IsInState(UsedNodeStates.ToVisit,nodeId))
                PropagateNode(nodeId);
        });
        Parallel.For(0, NodeStates.Length, nodeId =>
        {
            if (NodeStates.IsInState(UsedNodeStates.Visited,nodeId))
                Visitor.Visit(nodes[nodeId]);
        });
    }
}