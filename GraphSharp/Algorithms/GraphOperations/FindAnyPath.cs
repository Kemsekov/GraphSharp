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
    /// <summary>
    /// Finds any first found path between any two nodes. Much faster than Dijkstra path finding
    /// </summary>
    /// <returns>Path between two nodes. Empty list if path is not found.</returns>
    /// <param name="startNodeId">Start point</param>
    /// <param name="endNodeId">End point</param>
    /// <param name="condition">
    /// If path's edges need to follow some condition
    /// (for example avoid forbidden nodes/edges) then specify this argument. 
    /// If this condition fails then edge will not be used in resulting path.
    /// By default always returns true and passes all edges.
    /// </param>
    /// <param name="getWeight">
    /// Func to get edge weight. When null will use default edge weight property
    /// </param>
    /// <param name="pathType">What type of path to produce</param>
    /// /// <returns>Path between two nodes. Empty list if path is not found.</returns>
    public IPath<TNode> FindAnyPath(int startNodeId, int endNodeId,Predicate<EdgeSelect<TEdge>>? condition = null,Func<TEdge,double>? getWeight = null, PathType pathType = PathType.OutEdges)
    {
        getWeight ??= x=>x.Weight;
        var path = FindPathWithFirstEncounter(
            startNodeId,
            endNodeId,
            v => GetPropagator(v),
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, StructureBase,pathType){GetWeight = getWeight},
            condition)
        .GetPath(startNodeId, endNodeId);
        return path;
    }
    /// <summary>
    /// Concurrently finds any first found path between any two nodes. Much faster than Dijkstra path finding
    /// </summary>
    /// <inheritdoc cref="FindAnyPath"/>
    public IPath<TNode> FindAnyPathParallel(int startNodeId, int endNodeId, Predicate<EdgeSelect<TEdge>>? condition = null, Func<TEdge,double>? getWeight = null, PathType pathType = PathType.OutEdges)
    {
        getWeight ??= x=>x.Weight;
        var path = 
        FindPathWithFirstEncounter(
            startNodeId,
            endNodeId,
            v => GetParallelPropagator(v),
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, StructureBase,pathType){GetWeight = getWeight},
            condition)
        .GetPath(startNodeId, endNodeId);
        return path;
    }

    /// <summary>
    /// Using any <see cref="PathFinderBase{TNode,TEdge}"/> to find path between two nodes by stopping search 
    /// at first encounter of <paramref name="endNodeId"/>
    /// </summary>
    /// <param name="startNodeId">Start point</param>
    /// <param name="endNodeId">End point</param>
    /// <param name="condition">
    /// If path's edges need to follow some condition
    /// (for example avoid forbidden nodes/edges) then specify this argument. 
    /// If this condition falls then edge will not be used in resulting path.
    /// By default passes all edges.
    /// </param>
    /// <param name="minStepsCount">Min count of steps to do</param>
    /// <param name="maxStepsCount">Max count of steps to do</param>
    /// <param name="createPropagator">What propagator to use</param>
    /// <param name="createPathFinder">What path finder to use</param>
    /// <returns><see langword="PathFinderBase"/> that was used to find path.</returns>
    PathFinderBase<TNode, TEdge> FindPathWithFirstEncounter(
            int startNodeId,
            int endNodeId,
            Func<PathFinderBase<TNode, TEdge>, PropagatorBase<TNode, TEdge>> createPropagator,
            Func<PathFinderBase<TNode, TEdge>> createPathFinder,
            Predicate<EdgeSelect<TEdge>>? condition = null,
            int minStepsCount = -1,
            int maxStepsCount = int.MaxValue)
    {
        condition ??= edge => true;
        var pathFinder = createPathFinder();

        pathFinder.Condition = condition;
        pathFinder.StartNodeId = startNodeId;

        pathFinder.SelectEvent += edge =>
        {
            if (edge.TargetId == endNodeId) 
                pathFinder.Done = true;
        };
        var propagator = createPropagator(pathFinder);
        propagator.SetPosition(startNodeId);
        var pathType = pathFinder.PathType;
        if(pathType==PathType.Undirected)
            propagator.SetToIterateByBothEdges();
        if(pathType==PathType.OutEdges)
            propagator.SetToIterateByOutEdges();
        if(pathType==PathType.InEdges)
            propagator.SetToIterateByInEdges();
        int steps = 0;
        while (!pathFinder.Done || steps<minStepsCount)
        {
            propagator.Propagate();
            steps++;
            if(steps>maxStepsCount) break;
        }
        ReturnPropagator(propagator);
        return pathFinder;
    }
}