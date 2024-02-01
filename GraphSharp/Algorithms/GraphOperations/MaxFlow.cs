using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Adapters;
using GraphSharp.Exceptions;
using QuikGraph.Algorithms.MaximumFlow;
using QuikGraph.Algorithms.Ranking;

namespace GraphSharp.Graphs;

/// <summary>
/// Result of max flow algorithms
/// </summary>
public class MaxFlowResult<TEdge>
where TEdge : IEdge
{
    /// <summary>
    /// Id of node that was used as source
    /// </summary>
    public int SourceId{get;}
    /// <summary>
    /// Id of node that was used as sink
    /// </summary>
    public int SinkId{get;}
    /// <summary>
    /// Maximum flow computed
    /// </summary>
    public double MaxFlow{get;}
    /// <summary>
    /// Capacities used in current max flow result
    /// </summary>
    public IDictionary<TEdge, double> Capacities { get; }
    /// <summary>
    /// ResidualCapacities in current max flow result
    /// </summary>
    /// <value></value>
    public double ResidualCapacities(TEdge e)=>Capacities[e]-Flow[e];
    /// <summary>
    /// Flow that goes trough edge
    /// </summary>
    public IDictionary<TEdge, double> Flow { get; }
    /// <summary>
    /// Creates a new instance if max fow result
    /// </summary>
    public MaxFlowResult(IEnumerable<TEdge> edges,MaximumFlowAlgorithm<int, EdgeAdapter<TEdge>> result)
    {
        SourceId = result.Source;
        SinkId = result.Sink;
        MaxFlow = result.MaxFlow;
        Capacities = edges.ToDictionary(x=>x,x => result.Capacities(new(x)));
        Flow= edges.ToDictionary(x=>x,x=> result.Capacities(new(x))-result.ResidualCapacities[new(x)]);
    }
    /// <summary>
    /// Creates new max flow from a complete max flow result
    /// </summary>
    public MaxFlowResult(IEnumerable<TEdge> edges,int sourceId, int sinkId,double maxFlow,Func<TEdge, double> capacities, Func<TEdge, double> flow)
    {
        SourceId = sourceId;
        SinkId = sinkId;
        MaxFlow = maxFlow;
        Capacities = edges.ToDictionary(x=>x,x=>capacities(x));
        Flow = edges.ToDictionary(x=>x,x=>flow(x));
    }

}

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: write tests fot it
    /// <summary>
    /// Uses quik graph implementation to compute EdmondsKarp max flow.<br/>
    /// This max flow algorithm suited for graphs with positive capacities and flows.
    /// </summary>
    /// <param name="sourceId">
    /// Id of source node
    /// </param>
    /// <param name="sinkId">
    /// Id of sink node
    /// </param>
    /// <param name="getCapacity">
    /// Function to get edge capacity. By default uses edge "capacity" property
    /// </param>
    public MaxFlowResult<TEdge> MaxFlowEdmondsKarp(int sourceId, int sinkId, Func<TEdge, double>? getCapacity = null)
    {
        getCapacity ??= e => e.MapProperties().Capacity;
        var createEdge = (int vertex1, int vertex2) => new EdgeAdapter<TEdge>(Configuration.CreateEdge(Nodes[vertex1], Nodes[vertex2]));

        var quikGraph = StructureBase.Converter.ToMutableQuikGraph();
        var augmentor = new ReversedEdgeAugmentorAlgorithm<int, EdgeAdapter<TEdge>>(
            quikGraph,
            (v1, v2) => createEdge(v1, v2));
        augmentor.AddReversedEdges();
        var maxFlow = new EdmondsKarpMaximumFlowAlgorithm<int, EdgeAdapter<TEdge>>(
            quikGraph,
            x => getCapacity(x.GraphSharpEdge),
            (v1, v2) => createEdge(v1, v2),
            augmentor
            );
        maxFlow.Compute(sourceId, sinkId);

        //because this implementation works by adding new edges to graph
        //we need to remove added edges afterwards to restore original
        //graph state
        foreach (var e in augmentor.AugmentedEdges)
            Edges.Remove(e.GraphSharpEdge);
        return new(Edges,maxFlow);
    }

}