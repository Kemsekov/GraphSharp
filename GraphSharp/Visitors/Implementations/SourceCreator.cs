using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors;
/// <summary>
/// This visitor will create sources on assigned points so any path in graph will ends on some of this points
/// </summary>
public class SourceCreator<TNode, TEdge> : Visitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge<TNode>
{
    public const byte Proceed = 4;
    public const byte ToRemove = 8;
    /// <summary>
    /// This boolean will be set true on each call of <see cref="IPropagator{,}.Propagate"/> if algorithm inside of this visitor did anything at all.
    /// Use this to determine when to stop.
    /// </summary>
    public bool DidSomething = true;
    public override IPropagator<TNode, TEdge> Propagator { get; }
    public IGraph<TNode, TEdge> Graph { get; }
    public SourceCreator(IGraph<TNode, TEdge> graph)
    {
        Propagator = new ParallelPropagator<TNode, TEdge>(this, graph);
        this.Graph = graph;
    }

    public override void EndVisit()
    {
        for (int i = 0; i < Graph.Nodes.MaxNodeId + 1; i++)
            if (IsNodeInState(i, Proceed))
                SetNodeState(i, ToRemove);

    }

    public override bool Select(TEdge edge)
    {
        return !IsNodeInState(edge.Target.Id, Proceed | ToRemove);
    }

    public override void Visit(TNode node)
    {
        SetNodeState(node.Id, Proceed);

        var edges = Graph.Edges[node.Id];
        var toRemove = new List<TEdge>(edges.Count());
        foreach (var edge in edges)
        {
            if (IsNodeInState(edge.Target.Id, ToRemove))
                toRemove.Add(edge);
        }
        lock(Graph)
        foreach (var edge in toRemove)
            Graph.Edges.Remove(edge);

        DidSomething = true;
    }
}