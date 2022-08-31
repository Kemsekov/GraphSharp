using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
namespace GraphSharp.Visitors;

/// <summary>
/// This visitor will create sources on assigned points so any path in graph will ends on some of this points
/// </summary>
public class SourceCreator<TNode, TEdge> : Visitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public const byte Proceed = 4;
    public const byte ToRemove = 8;
    public override IPropagator<TNode, TEdge> Propagator { get; }
    public IGraph<TNode, TEdge> Graph { get; }
    public SourceCreator(IGraph<TNode, TEdge> graph)
    {
        Propagator = new ParallelPropagator<TNode, TEdge>(this, graph);
        this.Graph = graph;
    }

    public override void BeforeSelect()
    {
        DidSomething = false;
    }
    public override bool Select(TEdge edge)
    {
        return !IsNodeInState(edge.TargetId, Proceed | ToRemove);
    }

    public override void Visit(TNode node)
    {
        SetNodeState(node.Id, Proceed);

        var edges = Graph.Edges.OutEdges(node.Id);
        var toRemove = new List<TEdge>(edges.Count());
        foreach (var edge in edges)
        {
            if (IsNodeInState(edge.TargetId, ToRemove))
                toRemove.Add(edge);
        }
        lock(Graph)
        foreach (var edge in toRemove)
            Graph.Edges.Remove(edge);

        DidSomething = true;
    }
    public override void EndVisit()
    {
        for (int i = 0; i < Graph.Nodes.MaxNodeId + 1; i++)
            if (IsNodeInState(i, Proceed))
                SetNodeState(i, ToRemove);
        Steps++;
    }
}