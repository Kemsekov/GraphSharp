using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
    /// <summary>
    /// Finds any first found path between any two nodes. Much faster than <see cref="GraphOperation{,}.FindShortestPaths"/>
    /// </summary>
    /// <returns>Path between two nodes. Empty list if path is not found.</returns>
    public IList<TNode> FindAnyPath(int startNodeId, int endNodeId)
    {
        var anyPathFinder = new AnyPathFinder<TNode, TEdge>(startNodeId, endNodeId, _structureBase);
        var propagator = new Propagator<TNode, TEdge>(anyPathFinder, _structureBase);
        propagator.SetPosition(startNodeId);
        while (anyPathFinder.DidSomething && !anyPathFinder.Done)
        {
            anyPathFinder.DidSomething = false;
            propagator.Propagate();
        }
        var path = anyPathFinder.GetPath();
        return path;
    }
    /// <summary>
    /// Concurrently finds any first found path between any two nodes. Much faster than <see cref="GraphOperation{,}.FindShortestPaths"/>
    /// </summary>
    /// <returns>Path between two nodes. Empty list if path is not found.</returns>
    public IList<TNode> FindAnyPathParallel(int startNodeId, int endNodeId)
    {
        var anyPathFinder = new AnyPathFinder<TNode, TEdge>(startNodeId, endNodeId, _structureBase);
        var propagator = new ParallelPropagator<TNode, TEdge>(anyPathFinder, _structureBase);
        propagator.SetPosition(startNodeId);
        while (anyPathFinder.DidSomething && !anyPathFinder.Done)
        {
            anyPathFinder.DidSomething = false;
            propagator.Propagate();
        }
        return anyPathFinder.GetPath();
    }
}