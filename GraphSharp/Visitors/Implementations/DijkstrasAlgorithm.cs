using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Visitors;
public class DijkstrasAlgorithm<TNode, TEdge> : IVisitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge<TNode>
{
    /// <summary>
    /// _path[node] = parent 
    /// </summary>
    int[] _path;
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    float[] _pathLength;
    private Func<TEdge, float> _getWeight;
    IGraphStructure<TNode, TEdge> _graph;
    int _startNodeId;
    public bool DidSomething = true;
    /// <summary>
    /// Count of steps it took to calculate Dijkstra's Algorithm
    /// </summary>
    public int Steps { get; private set; }

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    public DijkstrasAlgorithm(int startNodeId, IGraphStructure<TNode, TEdge> graph, Func<TEdge,float>? getWeight = null)
    {
        getWeight ??= e=>e.Weight;
        this._getWeight = getWeight;
        this._graph = graph;
        this._startNodeId = startNodeId;
        _pathLength = new float[graph.Nodes.MaxNodeId+1];
        _path = new int[graph.Nodes.MaxNodeId+1];
        Array.Fill(_path,-1);
        Array.Fill(_pathLength,-1);
        _pathLength[startNodeId] = 0;
    }
    public void EndVisit()
    {
        this.Steps++;
    }

    public bool Select(TEdge connection)
    {
        bool updatePath = true;
        var pathLength = _pathLength[connection.Source.Id] + _getWeight(connection);

        var pathSoFar = _pathLength[connection.Target.Id];

        if (pathSoFar!=-1)
        {
            if (pathSoFar <= pathLength)
            {
                updatePath = false;
            }
        }
        if (updatePath)
        {
            _pathLength[connection.Target.Id] = pathLength;
            _path[connection.Target.Id] = connection.Source.Id;
            return true;
        }
        return false;
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
        if (_path[endNodeId]==-1) return path;
        while (true)
            {
                var parent = _path[endNodeId];
                if(parent==-1) break;
                path.Add(_graph.Nodes[endNodeId]);
                endNodeId = parent;
            }
        path.Add(this._graph.Nodes[_startNodeId]);
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