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
    public (IEnumerable<TNode> tour, double tourCost) TspOpt2(IEnumerable<TNode> tour, double tourCost)
    {
        var tsp = new Satsuma.Opt2Tsp<TNode>((n1, n2) => (n1.Position - n2.Position).Length(), tour, tourCost);
        tsp.Run();
        return (tsp.Tour, tsp.TourCost);
    }
    public (IEnumerable<TNode> tour, double tourCost) TspCheapestLinkOnPositions()
    {
        var xOrder = Nodes.OrderBy(x=>x.Position.X);
        var yOrder = Nodes.OrderBy(x=>x.Position.Y);
        xOrder.Aggregate((n1,n2)=>{
            var e = Configuration.CreateEdge(n1,n2);
            Edges.Add(e);
            return n2;
        });
        yOrder.Aggregate((n1,n2)=>{
            var e = Configuration.CreateEdge(n1,n2);
            Edges.Add(e);
            return n2;
        });
        // TODO: implement it
        return (Enumerable.Empty<TNode>(),0);
        throw new NotImplementedException();
    }
    public (IEnumerable<TNode> tour, double tourCost) TspCheapestLink(Func<TNode, TNode, double> cost)
    {
        var tsp = new Satsuma.CheapestLinkTsp<TNode>(Nodes.ToList(), cost);
        return (tsp.Tour, tsp.TourCost);
    }
    public (IEnumerable<TNode> tour, double tourCost) TspInsertionFarthest(Func<TNode, TNode, double> cost)
    {
        var tsp = new Satsuma.InsertionTsp<INode>(Nodes.Cast<INode>(), (n1, n2) => cost((TNode)n1, (TNode)n2), Satsuma.TspSelectionRule.Farthest);
        tsp.Run();
        return (tsp.Tour.Cast<TNode>(), tsp.TourCost);
    }
    public (IEnumerable<TNode> tour, double tourCost) TspInsertionNearest(Func<TNode, TNode, double> cost)
    {
        var tsp = new Satsuma.InsertionTsp<INode>(Nodes.Cast<INode>(), (n1, n2) => cost((TNode)n1, (TNode)n2), Satsuma.TspSelectionRule.Nearest);
        tsp.Run();
        return (tsp.Tour.Cast<TNode>(), tsp.TourCost);
    }
}