using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GraphSharp.Edges;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Visitors;
/// <summary>
/// Visitor that finds shortest path between given node and any other node in a graph.
/// </summary>
public class DijkstrasAlgorithm<TNode, TEdge> : IVisitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge<TNode>
{
    int[] _path;
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    float[] _pathLength;
    private Func<TEdge, float> _getWeight;
    IGraph<TNode, TEdge> _graph;
    /// <summary>
    /// _path[node] = parent 
    /// </summary>
    public int[] Path => _path;
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    public float[] PathLength => _pathLength;
    public bool DidSomething = true;
    /// <summary>
    /// Count of steps it took to calculate Dijkstra's Algorithm
    /// </summary>
    public int Steps;
    public int StartNodeId;

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    public DijkstrasAlgorithm(int startNodeId, IGraph<TNode, TEdge> graph, Func<TEdge, float>? getWeight = null)
    {
        getWeight ??= e => e.Weight;
        this._getWeight = getWeight;
        this._graph = graph;
        this.StartNodeId = startNodeId;
        _pathLength = new float[graph.Nodes.MaxNodeId + 1];
        _path = new int[graph.Nodes.MaxNodeId + 1];
        Array.Fill(_path, -1);
        Array.Fill(_pathLength, -1);
        _pathLength[startNodeId] = 0;
    }
    public void Clear(int startNodeId)
    {
        this.StartNodeId = startNodeId;
        _pathLength = new float[_graph.Nodes.MaxNodeId + 1];
        _path = new int[_graph.Nodes.MaxNodeId + 1];
        Array.Fill(_path, -1);
        Array.Fill(_pathLength, -1);
        _pathLength[startNodeId] = 0;
    }
    public void EndVisit()
    {
        this.Steps++;
    }

    public bool Select(TEdge connection)
    {
        var sourceId = connection.Source.Id;
        var targetId = connection.Target.Id;
        var pathLength = _pathLength[sourceId] + _getWeight(connection);

        var pathSoFar = _pathLength[targetId];

        if (pathSoFar != -1)
        {
            if (pathSoFar <= pathLength)
            {
                return false;
            }
        }
        _pathLength[targetId] = pathLength;
        _path[targetId] = sourceId;
        return true;
    }

    public void Visit(TNode node)
    {
        DidSomething = true;
    }

    /// <summary>
    /// Get path from start point to end point
    /// </summary>
    /// <returns>Empty list if path not found</returns>
    public IList<TNode> GetPath(int endNodeId)
    {
        var path = new List<TNode>();
        if (_path[endNodeId] == -1) return path;
        while (true)
        {
            var parent = _path[endNodeId];
            if (parent == -1) break;
            path.Add(_graph.Nodes[endNodeId]);
            endNodeId = parent;
        }
        path.Add(this._graph.Nodes[StartNodeId]);
        path.Reverse();
        return path;
    }
    /// <summary>
    /// Get path length to some done. -1 means there is no path exists
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public double GetPathLength(int nodeId)
    {
        return _pathLength[nodeId];
    }
}