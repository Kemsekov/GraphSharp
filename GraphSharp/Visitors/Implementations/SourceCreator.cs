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
public class SourceCreator<TNode, TEdge> : VisitorWithPropagator<TEdge>
where TNode : INode
where TEdge : IEdge
{

    const byte Proceed = 16;
    const byte ToRemove = 32;
    ByteStatesHandler NodeStates => Propagator.NodeStates;
    ///<inheritdoc/>
    public override PropagatorBase<TEdge> Propagator { get; }
    /// <summary>
    /// Graph that used by this class
    /// </summary>
    public IGraph<TNode, TEdge> Graph { get; }
    /// <summary>
    /// Creates a new instance of source creator
    /// </summary>
    public SourceCreator(IGraph<TNode, TEdge> graph)
    {
        Propagator = new ParallelPropagator<TEdge>(graph.Edges,this,graph.Nodes.MaxNodeId);
        this.Graph = graph;
    }

    ///<inheritdoc/>
    protected override void StartImpl()
    {
        DidSomething = false;
    }
    ///<inheritdoc/>
    protected override bool SelectImpl(EdgeSelect<TEdge> edge)
    {
        return !NodeStates.IsInState(Proceed | ToRemove,edge.TargetId);
    }

    ///<inheritdoc/>
    protected override void VisitImpl(int nodeId)
    {
        NodeStates.AddState(Proceed,nodeId);

        var edges = Graph.Edges.OutEdges(nodeId);
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
    ///<inheritdoc/>
    protected override void EndImpl()
    {
        for (int i = 0; i < Graph.Nodes.MaxNodeId + 1; i++)
            if (NodeStates.IsInState(Proceed,i))
                NodeStates.AddState(ToRemove,i);
        Steps++;
        if(!DidSomething) Done=true;
    }
}