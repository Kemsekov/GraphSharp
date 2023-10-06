using System;
using System.Collections;

namespace GraphSharp.Graphs;

/// <summary>
/// Max flow implementation that computes max flow in almost linear time yeah.<br/>
/// Also, it works on continuous flow values space, so results is not discrete, but flow can be any double value
/// </summary>
public class MaxFlowConvergence<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    IImmutableEdgeSource<TEdge> edges;
    Func<TEdge, double> capacities;
    /// <summary>
    /// Creates new instance of max flow convergence algorithm
    /// </summary>
    /// <param name="edges">graph edges to be used</param>
    /// <param name="capacities">edge capacities</param>
    public MaxFlowConvergence(IImmutableEdgeSource<TEdge> edges, Func<TEdge,double> capacities)
    {
        this.edges = edges;
        this.capacities = capacities;
        // this.propagator = new Propagators.Propagator<TNode,TEdge>()
    }

    void Step(){

    }
}
