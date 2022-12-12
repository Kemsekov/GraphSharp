using System;
using GraphSharp.Common;
using GraphSharp.Graphs;
namespace GraphSharp.Visitors;

/// <summary>
/// Algorithm to find length of shortest path between given node to all nodes in a graph.
/// Basically the same as <see cref="DijkstrasAlgorithm{TNode,TEdge}"/> but only keeps track of lengths of paths so uses(x2) less memory than DijkstrasAlgorithm and a bit faster.
/// </summary>
public class ShortestPathsLengthFinderAlgorithms<TNode, TEdge> : VisitorBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public Func<TEdge,double> GetWeight{get;set;}
    /// <summary>
    /// What is the length of path from startNode to some other node so far.  
    /// </summary>
    public RentedArray<double> PathLength{get;protected set;}
    IImmutableGraph<TNode, TEdge> Graph{get;}
    /// <summary>
    /// Root of all found path lengths
    /// </summary>
    /// <value></value>
    public int StartNodeId{get;protected set;}

    /// <param name="startNodeId">Node from which we need to find a shortest path</param>
    /// <param name="graph">Algorithm will be executed on this graph</param>
    public ShortestPathsLengthFinderAlgorithms(int startNodeId, IImmutableGraph<TNode, TEdge> graph)
    {
        GetWeight = e => e.Weight;
        this.Graph = graph;
        this.StartNodeId = startNodeId;
        PathLength = ArrayPoolStorage.RentArray<double>(graph.Nodes.MaxNodeId + 1);
        PathLength.Fill(-1);
        PathLength[startNodeId] = 0;
    }
    /// <summary>
    /// Disposes when collected by GC
    /// </summary>
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
    ///<inheritdoc/>
    protected override bool SelectImpl(EdgeSelect<TEdge> connection)
    {
        var sourceId = connection.SourceId;
        var targetId = connection.TargetId;
        var pathLength = PathLength[sourceId] + GetWeight(connection);

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

    ///<inheritdoc/>
    protected override void VisitImpl(TNode node)
    {
        DidSomething = true;
    }
    ///<inheritdoc/>
    protected override void EndImpl()
    {
        if(!DidSomething) Done = true;
    }

    ///<inheritdoc/>
    protected override void StartImpl()
    {

    }
}