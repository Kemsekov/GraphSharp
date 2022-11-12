using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Adapters;
using Satsuma;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Improves a solution for the computed TSP. <br/>
	/// It starts from a precomputed tour (e.g. one returned by TSPBubbleExpansion) and gradually improves it by 
	/// repeatedly swapping two edges.
    /// </summary>
    public ITsp<TNode> TspOpt2(Func<TNode, TNode, double> cost, IEnumerable<TNode> tour, double tourCost)
    {
        var tsp = new Satsuma.Opt2Tsp<TNode>(cost, tour, tourCost);
        tsp.Run();
        return tsp;
    }
    public ITsp<TNode> TspCheapestLinkOnPositions()
    {
        throw new NotImplementedException();
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