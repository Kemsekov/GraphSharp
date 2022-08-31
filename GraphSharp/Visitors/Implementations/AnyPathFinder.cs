using System;
using System.Collections.Generic;
using GraphSharp.Graphs;
namespace GraphSharp.Visitors;

/// <summary>
/// Finds any first found path between two nodes.
/// </summary>
public class AnyPathFinder<TNode, TEdge> : IVisitorWithSteps<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// _path[nodeId] = parentId <br/>
    /// _path[nodeId] == -1 when parent is not set <br/>
    /// </summary>
    int[] _path;
    /// <summary>
    /// Algorithm executed on this graph
    /// </summary>
    public IGraph<TNode, TEdge> Graph { get; }
    /// <summary>
    /// Id of first node in the path
    /// </summary>
    public int StartNodeId { get; protected set; }
    /// <summary>
    /// Id of last node in the path
    /// </summary>
    public int EndNodeId { get; protected set; }
    public bool Done{get;protected set;} = false;
    public bool DidSomething{get;protected set;} = true;
    public int Steps{get;protected set;} = 0;

    public AnyPathFinder(int startNodeId, int endNodeId, IGraph<TNode, TEdge> graph)
    {
        this.Graph = graph;
        this.StartNodeId = startNodeId;
        this.EndNodeId = endNodeId;
        _path = new int[graph.Nodes.MaxNodeId + 1];
        Array.Fill(_path, -1);
    }

    public void SetPosition(int startNodeId, int endNodeId)
    {
        this.StartNodeId = startNodeId;
        this.EndNodeId = endNodeId;
        Array.Fill(_path, -1);
    }

    public void BeforeSelect()
    {
        DidSomething = false;
    }
    public bool Select(TEdge edge)
    {
        if (Done) return false;
        if (edge.TargetId == EndNodeId)
        {
            Done = true;
        }
        if (_path[edge.TargetId] == -1)
        {
            _path[edge.TargetId] = edge.SourceId;
            return true;
        }
        return false;
    }

    public void Visit(TNode node)
    {
        DidSomething = true;
    }
    public void EndVisit()
    {
        Steps++;
    }
    /// <summary>
    /// Builds path between given nodes.
    /// </summary>
    public IList<TNode> GetPath()
    {
        var path = new List<TNode>();
        var endNodeId = EndNodeId;
        if (_path[endNodeId] == -1) return path;
        while (true)
        {
            var parent = _path[endNodeId];
            path.Add(Graph.Nodes[endNodeId]);
            if (parent == StartNodeId) break;
            endNodeId = parent;
        }
        path.Add(Graph.Nodes[StartNodeId]);
        path.Reverse();
        return path;
    }


}