using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors;
/// <summary>
/// Finds any first found path between two nodes.
/// </summary>
public class AnyPathFinder<TNode, TEdge> : IVisitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge<TNode>
{
    /// <summary>
    /// _path[nodeId] = parentId <br/>
    /// _path[nodeId] == -1 when parent is not set <br/>
    /// </summary>
    int[] _path;
    public IGraph<TNode, TEdge> Graph { get; }
    public int StartNodeId { get; protected set; }
    public int EndNodeId { get; protected set; }
    public bool Done = false;
    public bool DidSomething = true;
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

    public void EndVisit()
    {
    }

    public bool Select(TEdge edge)
    {
        if (Done) return false;
        if (edge.Target.Id == EndNodeId)
        {
            Done = true;
        }
        if (_path[edge.Target.Id] == -1)
        {
            _path[edge.Target.Id] = edge.Source.Id;
            return true;
        }
        return false;
    }

    public void Visit(TNode node)
    {
        DidSomething = true;
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