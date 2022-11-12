using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
namespace GraphSharp.Visitors;

/// <summary>
/// This visitor will create sources on assigned points so any path in graph will ends on some of this points
/// </summary>
public class SourceCreator<TNode, TEdge> : VisitorWithPropagator<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public const byte Proceed = 16;
    public const byte ToRemove = 32;
    ByteStatesHandler NodeStates => Propagator.NodeStates;
    public override PropagatorBase<TNode, TEdge> Propagator { get; }
    public IGraph<TNode, TEdge> Graph { get; }

    public SourceCreator(IGraph<TNode, TEdge> graph)
    {
        Propagator = new ParallelPropagator<TNode, TEdge>(this, graph);
        this.Graph = graph;
    }

    protected override void StartImpl()
    {
        DidSomething = false;
    }
    protected override bool SelectImpl(TEdge edge)
    {
        return !NodeStates.IsInState(Proceed | ToRemove,edge.TargetId);
    }

    protected override void VisitImpl(TNode node)
    {
        NodeStates.AddState(Proceed,node.Id);

        var edges = Graph.Edges.OutEdges(node.Id);
        var toRemove = new List<TEdge>(edges.Count());
        foreach (var edge in edges)
        {
            if (NodeStates.IsInState(ToRemove,edge.TargetId))
                toRemove.Add(edge);
        }
        lock(Graph)
        foreach (var edge in toRemove)
            Graph.Edges.Remove(edge);

        DidSomething = true;
    }
    protected override void EndImpl()
    {
        for (int i = 0; i < Graph.Nodes.MaxNodeId + 1; i++)
            if (NodeStates.IsInState(Proceed,i))
                NodeStates.AddState(ToRemove,i);
        Steps++;
        if(!DidSomething) Done=true;
    }
}