using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;

using GraphSharp.Graphs;

using GraphSharp.Propagators;

namespace GraphSharp.Visitors;
/// <summary>
/// This algorithm will try to find such a path between two nodes that each edge in that path satisfies specified condition
/// </summary>
public class AnyPathFinderWithCondition<TNode, TEdge> : IVisitor<TNode,TEdge>
where TNode : INode
where TEdge : IEdge
{
    public Predicate<TEdge> Condition { get; }
    AnyPathFinder<TNode,TEdge> pathFinder;
    public bool DidSomething{get=>pathFinder.DidSomething;set=>pathFinder.DidSomething=value;}
    public bool Done=>pathFinder.Done;
    public AnyPathFinderWithCondition(int startNodeId, int endNodeId, IGraph<TNode, TEdge> graph, Predicate<TEdge> condition)
    {
        Condition = condition;
        pathFinder = new(startNodeId,endNodeId,graph);
    }

    public void SetPosition(int startNodeId, int endNodeId)
    {
        pathFinder.SetPosition(startNodeId,endNodeId);
    }

    public void EndVisit()
    {
        pathFinder.EndVisit();
    }

    public bool Select(TEdge edge)
    {
        if(!Condition(edge)) return false;
        return pathFinder.Select(edge);
    }

    public void Visit(TNode node)
    {
        pathFinder.Visit(node);
    }
    /// <summary>
    /// Builds path between given nodes.
    /// </summary>
    public IList<TNode> GetPath()
    {
        return pathFinder.GetPath();
    }

}