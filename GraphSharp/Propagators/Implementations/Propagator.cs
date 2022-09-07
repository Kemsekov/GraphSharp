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
    /// <inheritdoc />
    public Propagator(IVisitor<TNode, TEdge> visitor, IGraph<TNode, TEdge> graph, bool reverseOrder=false) : base(visitor, graph,reverseOrder)
    {
    }
    protected override void PropagateNodes()
    {
        var nodes = Graph.Nodes;
        for (int nodeId = 0; nodeId < _nodeFlags.Length; ++nodeId)
        {
            if ((_nodeFlags[nodeId] & ToVisit) == ToVisit)
                PropagateNode(nodeId);
        };
        for (int nodeId = 0; nodeId < _nodeFlags.Length; ++nodeId)
        {
            if ((_nodeFlags[nodeId] & Visited) == Visited)
                Visitor.Visit(nodes[nodeId]);
        };
    }

}