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

    /// <param name="getWeight">Determine how to find a eccentricity of a node. By default it uses edges weights, but you can change it.</param>
    /// <returns>Length of a longest shortest path for a given node and endpoint of that path.</returns>
    public (float length, TNode farthestNode) FindEccentricity(int nodeId, Func<TEdge, float>? getWeight = null)
    {
        var pathFinder = new ShortestPathsLengthFinderAlgorithms<TNode, TEdge>(nodeId, _structureBase, getWeight);
        var propagator = new ParallelPropagator<TNode, TEdge>(pathFinder, _structureBase);
        propagator.SetPosition(nodeId);
        while (pathFinder.DidSomething)
        {
            pathFinder.DidSomething = false;
            propagator.Propagate();
        }
        var p = pathFinder.PathLength.Select((length, index) => (length, index)).MaxBy(x => x.length);
        return (p.length, Nodes[p.index]);
    }
}