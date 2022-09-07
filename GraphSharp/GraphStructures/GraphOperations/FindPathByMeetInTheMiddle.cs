using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    public IList<TNode> FindPathByMeetInTheMiddle(int startNodeId, int endNodeId, Predicate<TEdge>? condition = null)
    {
        return FindPathByMeetInTheMiddleBase(
            startNodeId,
            endNodeId,
            visitor => GetPropagator(visitor),
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, _structureBase),
            condition);
    }
    /// <summary>
    /// Concurrent path finder <br/>
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    /// </summary>
    public IList<TNode> FindPathByMeetInTheMiddleParallel(int startNodeId, int endNodeId, Predicate<TEdge>? condition = null)
    {
        return FindPathByMeetInTheMiddleBase(
            startNodeId,
            endNodeId,
            visitor => GetParallelPropagator(visitor),
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, _structureBase),
            condition);
    }
    /// <summary>
    /// Finds meet point between two nodes such that path nodes count of <br/>
    /// <paramref name="startNodeId"/> -> <paramref name="meetPoint"/> is equal to <br/>
    /// <paramref name="meetPoint"/> -> <paramref name="endNodeId"/>
    /// </summary>
    /// <param name="createPropagator">What propagator to use to find meet point</param>
    /// <param name="condition">What edges do we need to avoid in a path?</param>
    /// <returns>Id of node between <paramref name="startNodeId"/> and <paramref name="endNodeId"/>.
    /// If not found returns -1.</returns>
    public int FindMeetPoint(
        int startNodeId,
        int endNodeId,
        Func<IVisitor<TNode, TEdge>, PropagatorBase<TNode, TEdge>> createPropagator,
        Predicate<TEdge>? condition = null)
    {
        condition ??= new(edge => true);
        int meet = -1;
        byte Added = 4;
        var outMeetPointFinder = new ActionVisitor<TNode, TEdge>();
        var inMeetPointFinder = new ActionVisitor<TNode, TEdge>();

        var outPropagator = createPropagator(outMeetPointFinder);
        var inPropagator = createPropagator(inMeetPointFinder);
        outPropagator.SetPosition(startNodeId);
        inPropagator.SetPosition(endNodeId);

        inPropagator.ReverseOrder = true;

        outMeetPointFinder.Condition = edge=>{
            if(outPropagator.IsNodeInState(edge.TargetId,Added)) return false;
            return condition(edge);
        };
        inMeetPointFinder.Condition = edge=>{
            if(outPropagator.IsNodeInState(edge.SourceId,Added)) return false;
            return condition(edge);
        };

        outMeetPointFinder.VisitEvent += node =>
        {
            outPropagator.SetNodeState(node.Id, Added);
            if (inPropagator.IsNodeInState(node.Id, Added))
            {
                meet = node.Id;
            }
        };

        inMeetPointFinder.VisitEvent += node =>
        {
            inPropagator.SetNodeState(node.Id, Added);
            if (outPropagator.IsNodeInState(node.Id, Added))
            {
                meet = node.Id;
            }
        };

        while(meet==-1 && !outMeetPointFinder.Done && !inMeetPointFinder.Done){
            outPropagator.Propagate();
            inPropagator.Propagate();
        }
        ReturnPropagator(outPropagator);
        ReturnPropagator(inPropagator);
        return meet;
    }
    /// <summary>
    /// Finds any first found path between any two nodes using meet in the middle technique<br/>
    /// Much faster on certain type of graphs where BFS with each step takes exponentially
    /// more nodes to process.
    /// </summary>
    /// <returns>Path between two nodes. Empty list if path is not found.</returns>
    public IList<TNode> FindPathByMeetInTheMiddleBase(
        int startNodeId,
        int endNodeId,
        Func<IVisitor<TNode, TEdge>, PropagatorBase<TNode, TEdge>> createPropagator,
        Func<PathFinderBase<TNode, TEdge>> createPathFinder,
        Predicate<TEdge>? condition = null)
    {
        condition ??= new(edge => true);
        int meetId = FindMeetPoint(startNodeId,endNodeId,createPropagator,condition);
        if(meetId==-1) return new List<TNode>();
        var path1 = FindPathWithFirstEncounter(
            startNodeId,
            meetId,
            createPropagator,
            ()=>new AnyPathFinder<TNode,TEdge>(startNodeId,_structureBase),
            condition).GetPath(startNodeId,meetId);
        var path2 = FindPathWithFirstEncounter(
            meetId,
            endNodeId,
            createPropagator,
            ()=>new AnyPathFinder<TNode,TEdge>(startNodeId,_structureBase),
            condition).GetPath(meetId,endNodeId);
        
        return path1.Concat(path2.Skip(1)).ToList();
    }

}