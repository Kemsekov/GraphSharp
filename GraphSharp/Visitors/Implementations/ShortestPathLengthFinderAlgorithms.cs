using System;
using GraphSharp.Graphs;
namespace GraphSharp.Visitors;

/// <summary>
/// Algorithm to find length of shortest path between given node to all nodes in a graph.
/// Basically the same as <see cref="DijkstrasAlgorithm{,}"/> but only keeps track of lengths of paths so uses(x2) less memory than DijkstrasAlgorithm and a bit faster.
/// </summary>
public class ShortestPathsLengthFinderAlgorithms<TNode, TEdge> : IVisitorWithSteps<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// What is the length of path from startNode to some other node so far.  
    /// </summary>
    public float[] PathLength{get;protected set;}
    private Func<TEdge, float> _getWeight;
    IGraph<TNode, TEdge> Graph{get;}
    public bool DidSomething = true;
    public int Steps { get; protected set; }
    public int StartNodeId{get;protected set;}

    public bool Done => throw new NotImplementedException();

    bool IVisitorWithSteps<TNode, TEdge>.DidSomething => throw new NotImplementedException();

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    /// <param name="graph">Algorithm will be executed on this graph</param>
    public ShortestPathsLengthFinderAlgorithms(int startNodeId, IGraph<TNode, TEdge> graph, Func<TEdge, float>? getWeight = null)
    {
        getWeight ??= e => e.Weight;
        this._getWeight = getWeight;
        this.Graph = graph;
        this.StartNodeId = startNodeId;
        PathLength = new float[graph.Nodes.MaxNodeId + 1];
        Array.Fill(PathLength, -1);
        PathLength[startNodeId] = 0;
    }
    // TODO: add tests for Clear method 

    /// <summary>
    /// Clears state of an algorithm and reset it's startNodeId
    /// </summary>
    public void Clear(int startNodeId)
    {
        this.StartNodeId = startNodeId;
        PathLength = new float[Graph.Nodes.MaxNodeId + 1];
        Array.Fill(PathLength, -1);
        PathLength[startNodeId] = 0;
        Steps = 0;
        DidSomething = true;
    }
    public void BeforeSelect()
    {
        DidSomething = false;
    }

    public bool Select(TEdge connection)
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

    public void Visit(TNode node)
    {
        DidSomething = true;
    }
    public void EndVisit()
    {
        this.Steps++;
    }

}