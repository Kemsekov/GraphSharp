using Google.OrTools.Graph;
using Google.OrTools.ConstraintSolver;
using Unchase.Satsuma.TSP.Contracts;
using System.Linq;
using Google.OrTools.ModelBuilder;
using MathNet.Numerics;
using Unchase.Satsuma.Adapters;
using System.Collections.Generic;
using System;
using System.Collections.Immutable;
namespace GraphSharp.Graphs;
/// <summary>
/// Tsp google or tools extensions
/// </summary>
public static class ImmutableGraphOperationFeedbackArcSet
{

    /// <summary>
    /// Works on directed graphs <br/>
    /// Computes a such set of edges that when removed(or reversed) produces DAG. <br/>
    /// Edges will be chosen in such a way that tries to minimize total sum weight of chosen edges
    /// </summary>
    public static IEdgeSource<TEdge> FeedbackArcSet<TNode, TEdge>(this ImmutableGraphOperation<TNode, TEdge> g, Func<TEdge, double> weight)
    where TNode : INode
    where TEdge : IEdge
    {
        var gClone = g.StructureBase.CloneJustConfiguration();
        var edges = new DefaultEdgeSource<TEdge>(g.Edges);
        var Nodes = g.Nodes;
        var nCount = Nodes.Count();
        gClone.SetSources(Nodes, edges);


        var removedEdges = new DefaultEdgeSource<TEdge>();
        var sccs = gClone.Do.FindStronglyConnectedComponentsTarjan().Components.ToList();
        var cyclesGraph = g.StructureBase.CloneJustConfiguration();
        cyclesGraph.SetSources(Nodes);
        while (sccs.Count != nCount)
        {
            // System.Console.WriteLine("SCC count "+sccs.Count);
            //get approximate set of cycles
            foreach (var scc in sccs)
            {
                if (scc.nodes.Count() <= 1) continue;
                var (cyclesNodes, edges_) =
                    gClone.Do
                    .Induce(scc.nodes.Select(i => i.Id)).Do
                    .ApproxCyclesDirected(weight, 1);

                //on each cycle find shortest edge
                foreach (var nodes in cyclesNodes)
                {
                    var cycleEdges = gClone.Edges.InducedEdges(nodes.Select(n => n.Id));
                    RemoveShortestEdge(weight, gClone, removedEdges, cycleEdges);
                }
                //if no cycles is found, just remove shortest edge from scc
                if (edges_.Count == 0)
                {
                    var sccEdges = gClone.Edges.InducedEdges(scc.nodes.Select(n => n.Id));
                    RemoveShortestEdge(weight, gClone, removedEdges, sccEdges);
                }
            }
            sccs = gClone.Do.FindStronglyConnectedComponentsTarjan().Components.ToList();
        }
        return removedEdges;
    }
    /// <summary>
    /// Works on directed graphs <br/>
    /// Computes a such set of edges that when removed(or reversed) produces DAG. <br/>
    /// Edges will be chosen in such a way that tries to minimize total sum weight of chosen edges. <br/>
    /// This method will produce bigger(not optimal) feedback arc set compared to <see cref="FeedbackArcSet"/> but a lot faster
    /// </summary>
    public static IEdgeSource<TEdge> FeedbackArcSetFast<TNode, TEdge>(this ImmutableGraphOperation<TNode, TEdge> g, Func<TEdge, double> weight)
    where TNode : INode
    where TEdge : IEdge
    {
        var gClone = g.StructureBase.CloneJustConfiguration();
        var edges = new DefaultEdgeSource<TEdge>(g.Edges);
        var Nodes = g.Nodes;
        var nCount = Nodes.Count();
        gClone.SetSources(Nodes, edges);


        var removedEdges = new DefaultEdgeSource<TEdge>();
        var sccs = gClone.Do.FindStronglyConnectedComponentsTarjan().Components.ToList();
        var cyclesGraph = g.StructureBase.CloneJustConfiguration();
        cyclesGraph.SetSources(Nodes);
        while (sccs.Count != nCount)
        {
            // System.Console.WriteLine("SCC count "+sccs.Count);
            //get approximate set of cycles
            foreach (var scc in sccs)
            {
                if (scc.nodes.Count() <= 1) continue;
                //if no cycles is found, just remove shortest edge from scc
                var sccEdges = gClone.Edges.InducedEdges(scc.nodes.Select(n => n.Id));
                RemoveShortestEdge(weight, gClone, removedEdges, sccEdges);
            }
            sccs = gClone.Do.FindStronglyConnectedComponentsTarjan().Components.ToList();
        }
        return removedEdges;
    }

    private static void RemoveShortestEdge<TNode, TEdge>(Func<TEdge, double> weight, IGraph<TNode, TEdge> gClone, DefaultEdgeSource<TEdge> removedEdges, IEnumerable<TEdge> cycleEdges)
        where TNode : INode
        where TEdge : IEdge
    {
        var toRemove = cycleEdges.MinBy(weight);
        if (toRemove is null) return;
        //and remove shortest edge
        //so we break cycle
        removedEdges.Add(toRemove);
        gClone.Edges.Remove(toRemove);
    }
}