using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    public IPath<TNode> FindPathByMeetInTheMiddle(int startNodeId, int endNodeId, Func<TEdge, double>? getWeight = null, Predicate<EdgeSelect<TEdge>>? condition = null, bool undirected = false)
    {
        getWeight ??= x => x.Weight;
        condition ??= x => true;
        var pathType = undirected ? PathType.Undirected : PathType.OutEdges;
        var path = FindPathByMeetInTheMiddleBase(
            startNodeId,
            endNodeId,
            condition,
            visitor => GetPropagator(visitor),
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, StructureBase, pathType) { GetWeight = getWeight },
            undirected);
        return path;
    }
    /// <summary>
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    /// Finds shortest path between two points using Dijkstra algorithm and meet in the middle technique.
    /// </summary>
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    public IPath<TNode> FindPathByMeetInTheMiddleDijkstra(int startNodeId, int endNodeId, Func<TEdge, double>? getWeight = null, Predicate<EdgeSelect<TEdge>>? condition = null, bool undirected = false)
    {
        getWeight ??= x => x.Weight;
        condition ??= x => true;
        var pathType = undirected ? PathType.Undirected : PathType.OutEdges;
        var path = FindPathByMeetInTheMiddleBase(
            startNodeId,
            endNodeId,
            condition,
            visitor => GetPropagator(visitor),
            () => new DijkstrasAlgorithm<TNode, TEdge>(startNodeId, StructureBase, pathType) { GetWeight = getWeight },
            undirected);
        return path;
    }
    /// <summary>
    /// Concurrently finds shortest path between two points using Dijkstra algorithm and meet in the middle technique.
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    /// </summary>
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    public IPath<TNode> FindPathByMeetInTheMiddleDijkstraParallel(int startNodeId, int endNodeId, Func<TEdge, double>? getWeight = null, Predicate<EdgeSelect<TEdge>>? condition = null, bool undirected = false)
    {
        getWeight ??= x => x.Weight;
        condition ??= x => true;
        var pathType = undirected ? PathType.Undirected : PathType.OutEdges;
        var path = FindPathByMeetInTheMiddleBase(
            startNodeId,
            endNodeId,
            condition,
            visitor => GetParallelPropagator(visitor),
            () => new DijkstrasAlgorithm<TNode, TEdge>(startNodeId, StructureBase, pathType) { GetWeight = getWeight },
            undirected);
        return path;
    }
    /// <summary>
    /// Concurrent path finder <br/>
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    /// </summary>
    /// <inheritdoc cref="FindPathByMeetInTheMiddleBase" />
    public IPath<TNode> FindPathByMeetInTheMiddleParallel(int startNodeId, int endNodeId, Func<TEdge, double>? getWeight = null, Predicate<EdgeSelect<TEdge>>? condition = null, bool undirected = false)
    {
        getWeight ??= x => x.Weight;
        condition ??= x => true;
        var pathType = undirected ? PathType.Undirected : PathType.OutEdges;
        var path = FindPathByMeetInTheMiddleBase(
            startNodeId,
            endNodeId,
            condition,
            visitor => GetParallelPropagator(visitor),
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, StructureBase, pathType) { GetWeight = getWeight },
            undirected);
        return path; ;
    }
    /// <summary>
    /// Finds any first found path between any two nodes using meet in the middle technique<br/>
    /// Much faster on certain type of graphs where BFS with each step takes exponentially
    /// more nodes to process.
    /// </summary>
    /// <param name="startNodeId">Path start</param>
    /// <param name="endNodeId">Path end</param>
    /// <param name="condition">Use this predicate to exclude some parts of path that is forbidden for exploring. True if given edge is allowed to pass, else false</param>
    /// <param name="createPropagator"></param>
    /// <param name="createPathFinder"></param>
    /// <param name="undirected">Whatever resulting path must be undirected or directed</param>
    /// <returns></returns>
    /// <returns>Path between two nodes. Empty list if path is not found.</returns>
    IPath<TNode> FindPathByMeetInTheMiddleBase(
        int startNodeId,
        int endNodeId,
        Predicate<EdgeSelect<TEdge>> condition,
        Func<IVisitor<TNode, TEdge>, PropagatorBase<TNode, TEdge>> createPropagator,
        Func<PathFinderBase<TNode, TEdge>> createPathFinder,
        bool undirected)
    {
        bool done = false;
        int intersectionNodeId = -1;
        var pathJoiner = new ActionVisitor<TNode, TEdge>();
        var propagator = createPropagator(pathJoiner);

        byte StartNodeBFS = (byte)(PropagatorBase<TNode, TEdge>.BiggestUsedState * 2);
        byte EndNodeBFS = (byte)(StartNodeBFS * 2);

        var startFinder = createPathFinder();
        var endFinder = createPathFinder();
        propagator.SetPosition(startNodeId, endNodeId);
        var states = propagator.NodeStates;
        states.AddState(StartNodeBFS, startNodeId);
        states.AddState(EndNodeBFS, endNodeId);

        if (undirected)
        {
            propagator.SetToIterateByBothEdges();
        }
        else
        {
            propagator.NodeStates.RemoveStateFromAll(UsedNodeStates.IterateByInEdges | UsedNodeStates.IterateByOutEdges);
            propagator.SetToIterateByOutEdges(startNodeId);
            propagator.SetToIterateByInEdges(endNodeId);
        }
        pathJoiner.StartEvent += () =>
        {
            startFinder.Start();
            endFinder.Start();
        };
        pathJoiner.EndEvent += () =>
        {
            startFinder.End();
            endFinder.End();
        };
        pathJoiner.VisitEvent += node =>
        {
            if (done) return;
            var id = node.Id;
            if (states.IsInState(StartNodeBFS, id) && states.IsInState(EndNodeBFS, id))
            {
                lock (pathJoiner)
                {
                    done = true;
                    intersectionNodeId = id;
                }
            }
            startFinder.Visit(node);
            endFinder.Visit(node);
        };
        if (undirected)
            pathJoiner.Condition += (EdgeSelect<TEdge> edge) =>
            {
                if (done) return false;
                if(!condition(edge)) return false;
                if (states.IsInState(StartNodeBFS, edge.SourceId))
                {
                    states.AddState(StartNodeBFS, edge.TargetId);
                    return startFinder.Select(edge);
                }
                if (states.IsInState(EndNodeBFS, edge.SourceId))
                {
                    states.AddState(EndNodeBFS, edge.TargetId);
                    return endFinder.Select(edge);
                }
                return false;
            };
        else
            pathJoiner.Condition += (EdgeSelect<TEdge> edge) =>
            {
                if (done) return false;
                if (states.IsInState(StartNodeBFS, edge.SourceId))
                {
                    states.AddState((byte)(StartNodeBFS | UsedNodeStates.IterateByOutEdges), edge.TargetId);
                    return startFinder.Select(edge);
                }
                if (states.IsInState(EndNodeBFS, edge.SourceId))
                {
                    states.AddState((byte)(EndNodeBFS | UsedNodeStates.IterateByInEdges), edge.TargetId);
                    return endFinder.Select(edge);
                }
                return false;
            };

        while (!done)
        {
            propagator.Propagate();
        }
        var path1 = startFinder.GetPath(startNodeId, intersectionNodeId);
        var path2 = endFinder.GetPath(endNodeId, intersectionNodeId);
        var cost = path1.Cost + path2.Cost;
        var resultPath = path1.Path.Concat(path2.Path.Reverse().Skip(1)).ToList();
        var pathType = undirected ? PathType.Undirected : PathType.OutEdges;
        return new PathResult<TNode>(x => cost, resultPath, pathType);
    }

}