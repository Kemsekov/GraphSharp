using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Visitors;
public class DijkstrasAlgorithm<TNode, TEdge> : Visitor<TNode, TEdge>
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
    IGraphStructure<TNode, TEdge> _graph;
    TNode _startNode;
    public bool DidSomething = true;
    public override PropagatorBase<TNode, TEdge> Propagator { get; }

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    public DijkstrasAlgorithm(TNode startNode, IGraphStructure<TNode, TEdge> graph)
    {
        this._graph = graph;
        this._startNode = startNode;
        Propagator = new ParallelPropagator<TNode, TEdge>(this, graph);
        _pathLength = new float[graph.Nodes.MaxNodeId+1];
        _path = new int[graph.Nodes.MaxNodeId+1];
        Array.Fill(_path,-1);
        Array.Fill(_pathLength,-1);
        _pathLength[startNode.Id] = 0;
        SetPosition(startNode.Id);
    }
    public override void EndVisit()
    {
    }

    public override bool Select(TEdge connection)
    {
        bool updatePath = true;
        var pathLength = _pathLength[connection.Source.Id] + connection.Weight;

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

    public override void Visit(TNode node)
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
        path.Add(_startNode);
        path.Reverse();
        return path;
    }
    public double GetPathLength(int nodeId)
    {
        return _pathLength[nodeId];
    }
}