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
    /// Finds radius and center of graph. <br/>
    /// Operates in O(V^2 * logV + EV) time where V is a count of nodes and E is a count of edges
    /// </summary>
    /// <param name="getWeight">Determine how to find a center of a graph. By default it uses edges weights, but you can change it.</param>
    public (float radius, IEnumerable<TNode> center) FindCenter(Func<TEdge, float>? getWeight = null)
    {
        var radius = float.MaxValue;
        var Nodes = _structureBase.Nodes;
        var center = new List<TNode>();
        var pathFinder = new ShortestPathsLengthFinderAlgorithms<TNode, TEdge>(0, _structureBase, getWeight);
        var propagator = new ParallelPropagator<TNode, TEdge>(pathFinder, _structureBase);
        foreach (var n in Nodes)
        {
            pathFinder.Clear(n.Id);
            propagator.SetPosition(n.Id);
            pathFinder.DidSomething = true;
            while (pathFinder.DidSomething)
            {
                pathFinder.DidSomething = false;
                propagator.Propagate();
            }
            // pathFinder = _structureBase.Do.FindShortestPathsParallel(n.Id);
            var p = pathFinder.PathLength.Select((length, index) => (length, index)).MaxBy(x => x.length);
            if (p.length != 0)
                if (p.length < radius)
                {
                    radius = p.length;
                    center.Clear();
                }
            if (Math.Abs(p.length - radius) < float.Epsilon)
                center.Add(Nodes[n.Id]);

        }
        return (radius, center);

    }
}