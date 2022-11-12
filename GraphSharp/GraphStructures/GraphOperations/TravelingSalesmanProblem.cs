using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Adapters;
using Satsuma;

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

public partial class GraphOperation<TNode, TEdge>
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
        var tsp = new Satsuma.Opt2Tsp<TNode>(cost, tour, tourCost);
        tsp.Run();
        return tsp;
    }
    
    public ITsp<TNode> TspCheapestLinkOnPositions(Func<TNode,Vector2> getPos)
    {
        var treeDegree2 = FindSpanningTreeDegree2OnNodes(getPos);
        var graph = StructureBase.CloneJustConfiguration();
        graph.SetSources(Nodes,Edges);
        graph.SetSources(edges:treeDegree2.tree);
        graph.Do.MakeBidirected();
        var path = graph.Do.FindAnyPath(treeDegree2.ends[0].Id,treeDegree2.ends[1].Id);
        var additionalCost = (getPos(treeDegree2.ends[0])-getPos(treeDegree2.ends[1])).Length();
        var cost = path.Cost+additionalCost;
        path.Path.Add(treeDegree2.ends[0]);
        return new TspResult<TNode>(path.Path,cost);
    }
    public ITsp<TNode> TspCheapestLinkOnPositions(Func<TEdge,double> edgeCost,Action<IGraph<TNode, TEdge>> doDelaunayTriangulation)
    {
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
    public ITsp<TNode> TspCheapestLink(Func<TNode, TNode, double> cost)
    {
        var tsp = new Satsuma.CheapestLinkTsp<TNode>(Nodes.ToList(), cost);
        return tsp;
    }
    public ITsp<INode> TspInsertionFarthest(Func<TNode, TNode, double> cost)
    {
        var tsp = new Satsuma.InsertionTsp<INode>(Nodes.Cast<INode>(), (n1, n2) => cost((TNode)n1, (TNode)n2), Satsuma.TspSelectionRule.Farthest);
        tsp.Run();
        return tsp;
    }
    public ITsp<INode> TspInsertionNearest(Func<TNode, TNode, double> cost)
    {
        var tsp = new Satsuma.InsertionTsp<INode>(Nodes.Cast<INode>(), (n1, n2) => cost((TNode)n1, (TNode)n2), Satsuma.TspSelectionRule.Nearest);
        tsp.Run();
        return tsp;
    }
}