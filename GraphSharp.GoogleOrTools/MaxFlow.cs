using Google.OrTools.Graph;
using GraphSharp.Exceptions;

namespace GraphSharp.Graphs;
public static class ImmutableGraphOperationExtension
{
    //because google or tools is just way too heavy to use
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
    /// Function to get edge capacity. By default uses edge flow values
    /// </param>
    public static MaxFlowResult<TEdge> MaxFlowGoogleOrTools<TNode, TEdge>(this ImmutableGraphOperation<TNode,TEdge> g,int sourceId, int sinkId, Func<TEdge, int>? getCapacity = null)
    where TNode : INode
    where TEdge : IEdge
    {
        getCapacity ??= e => (int)e.MapProperties().Capacity;
        var Edges = g.Edges;
        var maxFlow = new MaxFlow();
        var edgeToId = new Dictionary<TEdge,int>();
        var idToEdge = new Dictionary<int,TEdge>();
        foreach(var (e,i) in Edges.Select((e,i)=>(e,i))){
            maxFlow.AddArcWithCapacity(e.SourceId,e.TargetId,getCapacity(e));
            edgeToId[e]=i;
            idToEdge[i]=e;
        }
        var status = maxFlow.Solve(sourceId, sinkId);
        if(status==MaxFlow.Status.BAD_INPUT)
            throw new FailedToSolveMaxFlowException("Bad graph input");
        if(status==MaxFlow.Status.BAD_RESULT)
            throw new FailedToSolveMaxFlowException("Bad result. Failed to solve max flow.");
        return new(Edges,sourceId,sinkId,maxFlow.OptimalFlow(),e=>getCapacity(e),edge=>maxFlow.Flow(edgeToId[edge]));
    }
}