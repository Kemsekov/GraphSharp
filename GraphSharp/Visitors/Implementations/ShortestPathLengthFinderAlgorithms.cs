using System;
using GraphSharp.Common;
using GraphSharp.Graphs;
namespace GraphSharp.Visitors;

/// <summary>
/// Algorithm to find length of shortest path between given node to all nodes in a graph.
/// Basically the same as <see cref="DijkstrasAlgorithm{,}"/> but only keeps track of lengths of paths so uses(x2) less memory than DijkstrasAlgorithm and a bit faster.
/// </summary>
public class ShortestPathsLengthFinderAlgorithms<TNode, TEdge> : VisitorBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// What is the length of path from startNode to some other node so far.  
    /// </summary>
    public RentedArray<double> PathLength{get;protected set;}
    private Func<TEdge, double> _getWeight;
    IImmutableGraph<TNode, TEdge> Graph{get;}
    public int StartNodeId{get;protected set;}

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    /// <param name="graph">Algorithm will be executed on this graph</param>
    public ShortestPathsLengthFinderAlgorithms(int startNodeId, IImmutableGraph<TNode, TEdge> graph, Func<TEdge, double>? getWeight = null)
    {
        getWeight ??= e => e.Weight;
        this._getWeight = getWeight;
        this.Graph = graph;
        this.StartNodeId = startNodeId;
        PathLength = ArrayPoolStorage.RentFloatArray(graph.Nodes.MaxNodeId + 1);
        PathLength.Fill(-1);
        PathLength[startNodeId] = 0;
    }
    ~ShortestPathsLengthFinderAlgorithms(){
        PathLength.Dispose();
    }
    /// <summary>
    /// Clears state of an algorithm and reset it's startNodeId
    /// </summary>
    public void Clear(int startNodeId)
    {
        this.StartNodeId = startNodeId;
        PathLength.Fill(-1);
        PathLength[startNodeId] = 0;
        Steps = 0;
        DidSomething = true;
        Done = false;
    }
    protected override bool SelectImpl(TEdge connection)
    {
        var sourceId = connection.SourceId;
        var targetId = connection.TargetId;
        var pathLength = PathLength[sourceId] + _getWeight(connection);

        var pathSoFar = PathLength[targetId];

        if (pathSoFar != -1)
        {
            if (pathSoFar <= pathLength)
            {
                return false;
            }
        }
        PathLength[targetId] = pathLength;
        return true;
    }

    protected override void VisitImpl(TNode node)
    {
        DidSomething = true;
    }
    protected override void EndImpl()
    {
        if(!DidSomething) Done = true;
    }

    protected override void StartImpl()
    {

    }
}