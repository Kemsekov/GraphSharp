using System;
using System.Collections.Generic;
using System.Linq;
using KdTree;
using MathNet.Numerics.Distributions;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: add sample to it and test on some graphs
    /// <summary>
    /// Method that uses nodes embeddings to determine graph isomorphism
    /// </summary>
    /// <returns>Value between 0 and 1 that gives confidence that current graph is isomorphic to another</returns>
    public double IsIsomorphic<TNode_, TEdge_>(IImmutableGraph<TNode_, TEdge_> another)
    where TNode_ : INode
    where TEdge_ : IEdge
    {
        if(Nodes.Count()!=another.Nodes.Count()) return 0;
        if(Edges.Count()!=another.Edges.Count()) return 0;

        var emb1 = StructureBase.Do.NodesEmbedding();
        var emb2 = another.Do.NodesEmbedding();

        var kdtree = new KdTree<double,int>(4,new KdTree.Math.DoubleMath());

        foreach (var n in emb1)
        {
            kdtree.Add(n.Value, n.Key);
        }

        var isomorphism = new Dictionary<int, int>();

        var differences = new List<double>();
        var differEdges = false;
        foreach (var n in another.Nodes)
        {
            var nEmb = emb2[n.Id];
            var closest = kdtree.GetNearestNeighbours(nEmb,1).First();
            isomorphism[closest.Value] = n.Id;

            var diff = Math.Sqrt(nEmb.Zip(closest.Point).Sum(v => (v.First - v.Second) * (v.First - v.Second)));
            differences.Add(diff);

            if (another.Edges.OutEdges(n.Id).Count() == Edges.OutEdges(closest.Value).Count() &&
                another.Edges.InEdges(n.Id).Count() == Edges.InEdges(closest.Value).Count()) continue;
            differEdges = true;
            break;
        }

        if (differEdges)
        {
            //not isomorphic
            return 0;
        }
        
        var mean = MathNet.Numerics.Statistics.Statistics.Mean(differences);
        var std = MathNet.Numerics.Statistics.Statistics.StandardDeviation(differences);// sqrt(V)


        var tMeanZero = mean / std * Math.Sqrt(differences.Count);
        var zeroMeanP = StudentT.CDF(0, 1, differences.Count - 1, tMeanZero);

        var stdDiff = differences.Select(i => Math.Abs(i - std)).ToList();
        var stdDiffMean = MathNet.Numerics.Statistics.Statistics.Mean(stdDiff);
        var stdDiffStd = MathNet.Numerics.Statistics.Statistics.StandardDeviation(stdDiff);
        var tStdZero = stdDiffMean / stdDiffStd * Math.Sqrt(stdDiff.Count);
        var stdZeroP = StudentT.CDF(0, 1, stdDiff.Count - 1, tStdZero);

        //we compute by T test p-values for difference of embeddings to have zero mean and zero std
        return zeroMeanP*stdZeroP;
    }

}