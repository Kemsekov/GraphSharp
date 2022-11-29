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
    /// Finds any first found path between any two nodes. Much faster than <see cref="GraphOperation{,}.FindShortestPaths"/>
    /// </summary>
    /// <returns>Path between two nodes. Empty list if path is not found.</returns>
    /// <param name="startNodeId">Start point</param>
    /// <param name="endNodeId">End point</param>
    /// <param name="condition">
    /// If path's edges need to follow some condition
    /// (for example avoid forbidden nodes/edges) then specify this argument. 
    /// If this condition falls then edge will not be used in resulting path
    /// By default passes all edges.
    /// </param>
    public IPath<TNode> FindAnyPath(int startNodeId, int endNodeId,Predicate<TEdge>? condition = null,Func<TEdge,double>? getWeight = null)
    {
        var path = FindPathWithFirstEncounter(
            startNodeId,
            endNodeId,
            v => GetPropagator(v),
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, StructureBase),
            condition)
        .GetPath(startNodeId, endNodeId);
        return new PathResult<TNode>(p=>StructureBase.ComputePathCost(p,getWeight),path);
    }
    /// <summary>
    /// Concurrently finds any first found path between any two nodes. Much faster than <see cref="GraphOperation{,}.FindShortestPaths"/>
    /// </summary>
    /// <returns>Path between two nodes. Empty list if path is not found.</returns>

    public IPath<TNode> FindAnyPathParallel(int startNodeId, int endNodeId, Predicate<TEdge>? condition = null, Func<TEdge,double>? getWeight = null)
    {
        var path = 
        FindPathWithFirstEncounter(
            startNodeId,
            endNodeId,
            v => GetParallelPropagator(v),
            () => new AnyPathFinder<TNode, TEdge>(startNodeId, StructureBase),
            condition)
            .GetPath(startNodeId, endNodeId);
        return new PathResult<TNode>(p=>StructureBase.ComputePathCost(p,getWeight),path);
    }
    
    /// <summary>
    /// Using any <see cref="PathFinderBase{,}"/> to find path between two nodes by stopping search 
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
    /// <param name="createPropagator">What propagator to use</param>
    /// <param name="createPathFinder">What path finder to use</param>
    /// <returns><paramref name="PathFinderBase"/> that was used to find path.</returns>
    PathFinderBase<TNode, TEdge> FindPathWithFirstEncounter(
            int startNodeId,
            int endNodeId,
            Func<PathFinderBase<TNode, TEdge>, PropagatorBase<TNode, TEdge>> createPropagator,
            Func<PathFinderBase<TNode, TEdge>> createPathFinder,
            Predicate<TEdge>? condition = null)
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
        while (!pathFinder.Done)
        {
            propagator.Propagate();
        }
        ReturnPropagator(propagator);
        return pathFinder;
    }
}