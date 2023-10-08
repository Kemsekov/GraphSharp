using System;
using System.Linq;
using GraphSharp.Adapters;
using QuikGraph.Algorithms.MaximumFlow;
using QuikGraph.Algorithms.Ranking;

namespace GraphSharp.Graphs;

/// <summary>
/// Result of max flow algorithms
/// </summary>
public class MaxFlowResult<TEdge>
where TEdge : IEdge
{
    MaximumFlowAlgorithm<int, EdgeAdapter<TEdge>> Result { get; }
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
    public Func<TEdge, double> Capacities { get; }
    /// <summary>
    /// ResidualCapacities in current max flow result
    /// </summary>
    /// <value></value>
    public Func<TEdge, double> ResidualCapacities { get; }
    /// <summary>
    /// Creates a new instance if max fow result
    /// </summary>
    public MaxFlowResult(MaximumFlowAlgorithm<int, EdgeAdapter<TEdge>> result)
    {
        Result = result;
        SourceId = result.Source;
        SinkId = result.Sink;
        MaxFlow = result.MaxFlow;
        ResidualCapacities = x => result.ResidualCapacities[new(x)];
        Capacities = x => Result.Capacities(new(x));
    }

}

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
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
    /// Function to get edge capacity. By default uses edge flow values
    /// </param>
    public MaxFlowResult<TEdge> MaxFlowEdmondsKarp(int sourceId, int sinkId, Func<TEdge, double>? getCapacity = null)
    {
        FIX THIS ONE
        throw new NotImplementedException("TODO");
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
        return new(maxFlow);
    }
}