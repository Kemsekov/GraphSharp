using System;
using System.Linq;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{

    /// <param name="nodeId">Node from which we need to find eccentricity</param>
    /// <param name="getWeight">Determine how to find a eccentricity of a node. By default it uses edges weights, but you can change it.</param>
    /// <returns>Length of a longest shortest path for a given node and endpoint of that path.</returns>
    public (double length, TNode farthestNode) FindEccentricity(int nodeId, Func<TEdge, double>? getWeight = null)
    {
        return FindEccentricityBase(
            nodeId,
            v=>GetPropagator(v),
            getWeight
        );
    }
    /// <param name="nodeId">Node from which we need to find eccentricity</param>
    /// <param name="getWeight">Determine how to find a eccentricity of a node. By default it uses edges weights, but you can change it.</param>
    /// <returns>Length of a longest shortest path for a given node and endpoint of that path.</returns>
    public (double length, TNode farthestNode) FindEccentricityParallel(int nodeId, Func<TEdge, double>? getWeight = null)
    {
        return FindEccentricityBase(
            nodeId,
            v=>GetParallelPropagator(v),
            getWeight
        );
    }

    (double length, TNode farthestNode) FindEccentricityBase(int nodeId,Func<ShortestPathsLengthFinderAlgorithms<TNode, TEdge>,IPropagator<TNode,TEdge>> createPropagator, Func<TEdge, double>? getWeight = null){
        getWeight ??= x=>x.Weight;
        var pathFinder = new ShortestPathsLengthFinderAlgorithms<TNode, TEdge>(nodeId, StructureBase){GetWeight = getWeight};
        var propagator = createPropagator(pathFinder);
        propagator.SetPosition(nodeId);
        while (!pathFinder.Done)
        {
            propagator.Propagate();
        }
        var p = pathFinder.PathLength.Select((length, index) => (length, index)).MaxBy(x => x.length);
        ReturnPropagator(propagator);
        return (p.length, Nodes[p.index]);
    }
}