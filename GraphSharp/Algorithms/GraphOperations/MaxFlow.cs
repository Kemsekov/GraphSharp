using System;
using System.Linq;
using GraphSharp.Adapters;
using QuikGraph.Algorithms.MaximumFlow;
using QuikGraph.Algorithms.Ranking;

namespace GraphSharp.Graphs;

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
    public EdmondsKarpMaximumFlowAlgorithm<int, EdgeAdapter<TEdge>> MaxFlowEdmondsKarp(int sourceId, int sinkId,Func<TEdge,double>? getCapacity = null)
    {
        getCapacity ??= e=>e.Weight;
        var createEdge = (int vertex1, int vertex2) =>new EdgeAdapter<TEdge>(Configuration.CreateEdge(Nodes[vertex1],Nodes[vertex2]));
        
        var quikGraph = StructureBase.Converter.ToMutableQuikGraph();
        var augmentor = new ReversedEdgeAugmentorAlgorithm<int, EdgeAdapter<TEdge>>(
            quikGraph, 
            (v1,v2)=>createEdge(v1,v2));
        augmentor.AddReversedEdges();
        var maxFlow = new EdmondsKarpMaximumFlowAlgorithm<int, EdgeAdapter<TEdge>>(
            quikGraph,
            x => getCapacity(x.GraphSharpEdge),
            (v1,v2)=>createEdge(v1,v2),
            augmentor
            );
        
        maxFlow.Compute(sourceId,sinkId);
        
        //because this implementation works by adding new edges to graph
        //we need to remove added edges afterwards to restore original
        //graph state
        foreach(var e in augmentor.AugmentedEdges)
            Edges.Remove(e.GraphSharpEdge);
        
        return maxFlow;
    }
}