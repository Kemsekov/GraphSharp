using Google.OrTools.Graph;
using GraphSharp.Exceptions;

namespace GraphSharp.Graphs;
public static class ImmutableGraphOperationMaxFlow
{
    /// <summary>
    /// Uses google or tools to compute max flow
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
    public static MaxFlowResult<TEdge> MaxFlowGoogleOrTools<TNode, TEdge>(this ImmutableGraphOperation<TNode,TEdge> g,int sourceId, int sinkId, Func<TEdge, long>? getCapacity = null)
    where TNode : INode
    where TEdge : IEdge
    {
        getCapacity ??= e => (long)e.MapProperties().Capacity;
        var Edges = g.Edges;
        var maxFlow = new MaxFlow();
        var edgeToId = new Dictionary<TEdge,int>();
        foreach(var (e,i) in Edges.Select((e,i)=>(e,i))){
            maxFlow.AddArcWithCapacity(e.SourceId,e.TargetId,getCapacity(e));
            edgeToId[e]=i;
        }
        var status = maxFlow.Solve(sourceId, sinkId);

        if(status!=MaxFlow.Status.OPTIMAL)
            throw new FailedToSolveMaxFlowException("Failed to find max flow. Error : "+status);
        
        return new(Edges,sourceId,sinkId,maxFlow.OptimalFlow(),e=>getCapacity(e),edge=>maxFlow.Flow(edgeToId[edge]));
    }
    /// <summary>
    /// Uses google or tools to compute min cost max flow
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
    /// <param name="getUnitCost">
    /// Function to get edge cost by one flow. By default uses edge "cost" property
    /// </param>
    /// <param name="getSupply">
    /// Function to get node supply. <br/>
    /// Positive values is nodes that gives flow, negative values is nodes that consumes flow. <br/>
    /// By default uses node "supply" property
    /// </param>
    public static MinCostFlowResult<TEdge> MinCostMaxFlowGoogleOrTools<TNode, TEdge>(this ImmutableGraphOperation<TNode,TEdge> g,Func<TEdge, long>? getCapacity = null,Func<TEdge, long>? getUnitCost = null,Func<TNode,long>? getSupply = null)
    where TNode : INode
    where TEdge : IEdge
    {
        getCapacity ??= e => (long)e.MapProperties().Capacity;
        getUnitCost ??= e => (long)e.MapProperties().Weight;
        getSupply ??= n => (long)n.MapProperties().Supply;
        
        var Edges = g.Edges;
        var Nodes = g.Nodes;

        var maxFlow = new MinCostFlow();
        var edgeToId = new Dictionary<TEdge,int>();
        foreach(var (e,i) in Edges.Select((e,i)=>(e,i))){
            maxFlow.AddArcWithCapacityAndUnitCost(e.SourceId,e.TargetId,getCapacity(e),getUnitCost(e));
            edgeToId[e]=i;
        }
        foreach(var n in Nodes){
            maxFlow.SetNodeSupply(n.Id,getSupply(n));
        }
        
        var status = maxFlow.Solve();

        if(status!=MinCostFlowBase.Status.OPTIMAL)
            throw new FailedToSolveMaxFlowException("Failed to solve min cost max flow. Error : "+status);
        
        return new(
            Nodes.Select(n=>n.Id),
            Edges,
            maxFlow.MaximumFlow(),
            maxFlow.OptimalCost(),
            e=>getCapacity(e),
            e=>getUnitCost(e),
            edge=>maxFlow.Flow(edgeToId[edge]),
            n=>getSupply(Nodes[n]));
    }
}