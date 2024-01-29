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
            //in each strongly connected component
            foreach (var scc in sccs)
            {
                var sccNodes = scc.nodes.Select(n=>n.Id).ToList();
                if(sccNodes.Count==1) continue;

                //get approximate set of cycles
                var cyclesNodes = 
                    gClone.Do
                    .Induce(sccNodes).Do          //induce current scc
                    .FindCyclesBasis(); //find cycles on induced scc

                //on each cycle find shortest edge
                foreach (var nodes in cyclesNodes)
                {
                    var cycleEdges = gClone.Edges.InducedEdges(nodes.Select(n => n.Id));
                    var toRemove = cycleEdges.MinBy(weight);
                    if (toRemove is null) continue;
                    //and remove shortest edge
                    //so we break cycle
                    removedEdges.Add(toRemove);
                    gClone.Edges.Remove(toRemove);
                }
            }
            sccs = gClone.Do.FindStronglyConnectedComponentsTarjan().Components.ToList();
        }
        return removedEdges;
    }
}