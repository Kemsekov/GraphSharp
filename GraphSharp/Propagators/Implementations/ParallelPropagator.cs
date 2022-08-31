using System.Threading.Tasks;
using GraphSharp.Graphs;
using GraphSharp.Visitors;
namespace GraphSharp.Propagators;

/// <summary>
/// Concurrent <see cref="PropagatorBase{,}"/> implementation.<br/> 
/// Every <see cref="IVisitor{,}"/> that accompany this propagator must be implemented
/// as thread-safe one.
/// </summary>
public class ParallelPropagator<TNode, TEdge> : PropagatorBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public ParallelPropagator(IVisitor<TNode, TEdge> visitor, IGraph<TNode, TEdge> graph) : base(visitor, graph)
    {
    }
    protected override void PropagateNodes()
    {
        var nodes = Graph.Nodes;
        Parallel.For(0, _nodeFlags.Length, nodeId =>
        {
            if ((_nodeFlags[nodeId] & ToVisit) == ToVisit)
                PropagateNode(nodeId);
        });
        Parallel.For(0, _nodeFlags.Length, nodeId =>
        {
            if ((_nodeFlags[nodeId] & Visited) == Visited)
                Visitor.Visit(nodes[nodeId]);
        });
    }
}