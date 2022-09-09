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
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, StructureBase),
            condition);
    }
    /// <summary>
    /// Finds shortest path between two points using Dijkstra algorithm and meet in the middle technique.
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    /// </summary>
    public IList<TNode> FindPathByMeetInTheMiddleDijkstra(int startNodeId, int endNodeId, Predicate<TEdge>? condition = null)
    {
        return FindPathByMeetInTheMiddleBase(
            startNodeId,
            endNodeId,
            visitor => GetPropagator(visitor),
            () => new DijkstrasAlgorithm<TNode, TEdge>(startNodeId, StructureBase),
            condition);
    }
    /// <summary>
    /// Concurrently finds shortest path between two points using Dijkstra algorithm and meet in the middle technique.
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    /// </summary>
    public IList<TNode> FindPathByMeetInTheMiddleDijkstraParallel(int startNodeId, int endNodeId, Predicate<TEdge>? condition = null)
    {
        return FindPathByMeetInTheMiddleBase(
            startNodeId,
            endNodeId,
            visitor => GetParallelPropagator(visitor),
            () => new DijkstrasAlgorithm<TNode, TEdge>(startNodeId, StructureBase),
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
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, StructureBase),
            condition);
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
        const byte IterateByInEdges = PropagatorBase<TNode, TEdge>.IterateByInEdges;
        int meetNodeId = -1;
        var outPathFinder = createPathFinder();
        var inPathFinder = createPathFinder();

        outPathFinder.StartNodeId = startNodeId;
        inPathFinder.StartNodeId = endNodeId;

        inPathFinder.GetEdgeDirection = edge=>(edge.TargetId,edge.SourceId);

        var meetInTheMiddlePathFinder = new ActionVisitor<TNode,TEdge>();

        var propagator = createPropagator(meetInTheMiddlePathFinder);

        propagator.SetPosition(startNodeId,endNodeId);

        propagator.SetNodeState(endNodeId,IterateByInEdges);

        meetInTheMiddlePathFinder.Condition = edge=>{
            if(!condition(edge)) return false;
            if(propagator.IsNodeInState(edge.TargetId,IterateByInEdges)){
                if(inPathFinder.Select(edge)){
                    propagator.SetNodeState(edge.SourceId,IterateByInEdges);
                    return true;
                }
                return false;
            }
            return outPathFinder.Select(edge);
        };
        meetInTheMiddlePathFinder.VisitEvent += node=>{
            if(outPathFinder.Path[node.Id]!=-1 && inPathFinder.Path[node.Id]!=-1){
                meetNodeId = node.Id;
            }
        };
        while(meetNodeId==-1) 
            propagator.Propagate();
        
        var p1 = outPathFinder.GetPath(startNodeId,meetNodeId);
        var p2 = inPathFinder.GetPath(endNodeId,meetNodeId);
        return p1.Concat(p2.Reverse().Skip(1)).ToList();
    }

}