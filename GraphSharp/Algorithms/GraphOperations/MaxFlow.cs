using System;
using System.Linq;
using GraphSharp.Adapters;
using QuikGraph.Algorithms.MaximumFlow;
using QuikGraph.Algorithms.Ranking;

namespace GraphSharp.Graphs;

public class MaxFlowResult<TEdge>
where TEdge : IEdge
{
    MaximumFlowAlgorithm<int, EdgeAdapter<TEdge>> Result { get; }
    public int SourceId{get;}
    public int SinkId{get;}
    public double MaxFlow{get;}
    public Func<TEdge, double> Capacities { get; }
    public Func<TEdge, double> ResidualCapacities { get; }
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
    /// Function to get edge capacity. By default uses edge weight.
    /// </param>
    public MaxFlowResult<TEdge> MaxFlowEdmondsKarp(int sourceId, int sinkId, Func<TEdge, double>? getCapacity = null)
    {
        getCapacity ??= e => e.Weight;
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