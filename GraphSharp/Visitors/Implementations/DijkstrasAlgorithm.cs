using System;
using System.Collections.Generic;
using GraphSharp.Graphs;
namespace GraphSharp.Visitors;

/// <summary>
/// Visitor that finds all shortest paths between given node to all other nodes in a graph.
/// </summary>
public class DijkstrasAlgorithm<TNode, TEdge> : IVisitorWithSteps<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    private Func<TEdge, float> _getWeight;
    IGraph<TNode, TEdge> _graph;
    /// <summary>
    /// Path[node] = parent 
    /// </summary>
    public int[] Path;
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    public float[] PathLength;
    public bool DidSomething{get;set;} = true;
    public bool Done{get;set;}
    public int Steps{get;protected set;}
    /// <summary>
    /// Each found path will begin from node with this id
    /// </summary>
    public int StartNodeId;

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    /// <param name="graph">Algorithm will be executed on this graph</param>
    public DijkstrasAlgorithm(int startNodeId, IGraph<TNode, TEdge> graph, Func<TEdge, float>? getWeight = null)
    {
        getWeight ??= e => e.Weight;
        this._getWeight = getWeight;
        this._graph = graph;
        this.StartNodeId = startNodeId;
        PathLength = new float[graph.Nodes.MaxNodeId + 1];
        Path = new int[graph.Nodes.MaxNodeId + 1];
        Array.Fill(Path, -1);
        Array.Fill(PathLength, -1);
        PathLength[startNodeId] = 0;
    }
    /// <summary>
    /// Clears state of an algorithm and reset it's startNodeId
    /// </summary>
    public void Clear(int startNodeId)
    {
        this.StartNodeId = startNodeId;
        PathLength = new float[_graph.Nodes.MaxNodeId + 1];
        Path = new int[_graph.Nodes.MaxNodeId + 1];
        Array.Fill(Path, -1);
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
        Path[targetId] = sourceId;
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
    /// <summary>
    /// Get path from start point to end point
    /// </summary>
    /// <returns>Empty list if path not found</returns>
    public IList<TNode> GetPath(int endNodeId)
    {
        var path = new List<TNode>();
        if (Path[endNodeId] == -1) return path;
        while (true)
        {
            var parent = Path[endNodeId];
            if (parent == -1) break;
            path.Add(_graph.Nodes[endNodeId]);
            endNodeId = parent;
        }
        path.Add(this._graph.Nodes[StartNodeId]);
        path.Reverse();
        return path;
    }

}