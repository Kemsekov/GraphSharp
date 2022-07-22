using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Graphs;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors;
public class ShortestPathsLengthFinderAlgorithms<TNode, TEdge> : IVisitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge<TNode>
{
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    public float[] PathLength =>_pathLength;
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    float[] _pathLength;
    private Func<TEdge, float> _getWeight;
    IGraph<TNode, TEdge> _graph;
    int _startNodeId;
    public bool DidSomething = true;
    /// <summary>
    /// Count of steps it took to calculate Dijkstra's Algorithm
    /// </summary>
    public int Steps { get; private set; }
    public int StartNodeId => _startNodeId;

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    /// <param name="getWeight">When null shortest path is computed by comparing weights of the edges. If you need to change this behavior specify this delegate. Beware that this method will be called in concurrent context and must be thread safe.</param>
    public ShortestPathsLengthFinderAlgorithms(int startNodeId, IGraph<TNode, TEdge> graph, Func<TEdge,float>? getWeight = null)
    {
        getWeight ??= e=>e.Weight;
        this._getWeight = getWeight;
        this._graph = graph;
        this._startNodeId = startNodeId;
        _pathLength = new float[graph.Nodes.MaxNodeId+1];
        Array.Fill(_pathLength,-1);
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
        return true;
    }

    public void Visit(TNode node)
    {
        DidSomething = true;
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