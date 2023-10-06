using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Threading.Tasks;
using GraphSharp.Adapters;
using Unchase.Satsuma;
using Unchase.Satsuma.TSP.Contracts;
using Unchase.Satsuma.TSP;

namespace GraphSharp.Graphs;

class TspResult<TNode> : ITsp<TNode>
{
    public IEnumerable<TNode> Tour{get;}
    public double TourCost{get;}
    public TspResult(IEnumerable<TNode> tour, double cost)
    {
        
        Tour = tour;
        TourCost = cost;
    }
}

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Improves a solution for the computed TSP. <br/>
	/// It starts from a precomputed tour (e.g. one returned by TSPBubbleExpansion) and gradually improves it by 
	/// repeatedly swapping two edges.
    /// </summary>
    public ITsp<TNode> TspOpt2(IEnumerable<TNode> tour, double tourCost, Func<TNode, TNode, double> cost)
    {
        var tsp = new Opt2Tsp<TNode>(cost, tour, tourCost);
        tsp.Run();
        return tsp;
    }
    /// <summary>
    /// Computes a TSP on 2vector positions
    /// </summary>
    public ITsp<TNode> TspCheapestLinkOnPositions(Func<TNode,Vector> getPos)
    {
        var count = Nodes.Count();
        if(count==1) return new TspResult<TNode>(new[]{Nodes.First(),Nodes.First()},0);
        if(count<=0) throw new ArgumentException("Cannot find TSP on empty graph");
        var treeDegree2 = FindSpanningTreeDegree2OnNodes(getPos);
        var graph = StructureBase.CloneJustConfiguration();
        graph.SetSources(Nodes,Edges);
        graph.SetSources(edges:treeDegree2.tree);
        graph.Do.MakeBidirected();
        var path = graph.Do.FindAnyPath(treeDegree2.ends[0].Id,treeDegree2.ends[1].Id);
        var additionalCost = (getPos(treeDegree2.ends[0])-getPos(treeDegree2.ends[1])).L2Norm();
        var cost = path.Cost+additionalCost;
        path.Path.Add(treeDegree2.ends[0]);
        return new TspResult<TNode>(path.Path,cost);
    }
    /// <summary>
    /// Computes tsp on edge costs only. When supplied with good <see langword="doDelaunayTriangulation"/> 
    /// function can compute TSP on any dimensional space.
    /// </summary>
    /// <param name="edgeCost">Function to get edge cost</param>
    /// <param name="doDelaunayTriangulation">
    /// Function that need to be able to connect closest nodes in given 
    /// graph by creating edge between them<br/>
    /// Delaunay triangulation works best, but you can try other variance.
    /// </param>
    /// <returns></returns>
    public ITsp<TNode> TspCheapestLinkOnEdgeCost(Func<TEdge,double> edgeCost,Action<IGraph<TNode, TEdge>> doDelaunayTriangulation)
    {
        var count = Nodes.Count();
        if(count==1) return new TspResult<TNode>(new[]{Nodes.First(),Nodes.First()},0);
        if(count<=0) throw new ArgumentException("Cannot find TSP on empty graph");
        var treeDegree2 = FindSpanningTreeDegree2OnNodes(edgeCost,doDelaunayTriangulation);
        var ends = treeDegree2.ends;
        var graph = StructureBase.CloneJustConfiguration();
        graph.SetSources(Nodes,Edges);
        graph.SetSources(edges:treeDegree2.tree);
        graph.Do.MakeBidirected();
        var path = graph.Do.FindAnyPath(treeDegree2.ends[0].Id,treeDegree2.ends[1].Id);
        var closingEdge = graph.Configuration.CreateEdge(ends[0],ends[1]);
        graph.Edges.Add(closingEdge);
        var cost = path.Cost+edgeCost(closingEdge);
        path.Path.Add(treeDegree2.ends[0]);
        return new TspResult<TNode>(path.Path,cost);
    }
    /// <summary>
    /// Computes TSP by cheapest link strategy. <br/>
    /// Creates a close-connected graph, finds on it spanning tree degree 2, builds tsp out of it and optimizes result.<br/>
    /// Works in O(N^2) time
    /// </summary>
    public ITsp<TNode> TspCheapestLink(Func<TNode, TNode, double> cost, int initialConnectionsCount)
    {
        var tsp = TspCheapestLinkOnEdgeCost(e=>e.MapProperties().Weight,g=>g.Do.ConnectToClosest(initialConnectionsCount,cost));;
        tsp = TspOpt2(tsp.Tour,tsp.TourCost,cost);
        return tsp;
    }

    /// <summary>
    /// Computes tsp by inserting farthest strategy
    /// </summary>
    /// <param name="cost"></param>
    public ITsp<INode> TspInsertionFarthest(Func<TNode, TNode, double> cost)
    {
        var tsp = new InsertionTsp<INode>(Nodes.Cast<INode>(), (n1, n2) => cost((TNode)n1, (TNode)n2), Unchase.Satsuma.TSP.Enums.TspSelectionRule.Farthest);
        tsp.Run();
        return tsp;
    }
    /// <summary>
    /// Computes tsp by inserting nearest strategy
    /// </summary>
    /// <param name="cost"></param>
    public ITsp<INode> TspInsertionNearest(Func<TNode, TNode, double> cost)
    {
        var tsp = new InsertionTsp<INode>(Nodes.Cast<INode>(), (n1, n2) => cost((TNode)n1, (TNode)n2), Unchase.Satsuma.TSP.Enums.TspSelectionRule.Nearest);
        tsp.Run();
        return tsp;
    }
}