using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors;
public class SourceCreator<TNode, TEdge> : Visitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge<TNode>
{
    public const byte Proceed = 4;
    public const byte ToRemove = 8;
    public bool DidSomething = true;
    public override IPropagator<TNode, TEdge> Propagator { get; }
    public IGraphStructure<TNode, TEdge> Graph { get; }
    public SourceCreator(IGraphStructure<TNode, TEdge> graph)
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
        var toRemove = new List<TEdge>();
        foreach (var edge in edges)
        {
            if (IsNodeInState(edge.Target.Id, ToRemove))
                toRemove.Add(edge);
        }

        foreach (var edge in toRemove)
            Graph.Edges.Remove(edge);

        DidSomething = true;
    }
}