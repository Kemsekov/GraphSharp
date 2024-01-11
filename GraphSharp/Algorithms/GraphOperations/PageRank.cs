using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Adapters;
using GraphSharp.Algorithms;
using QuikGraph.Algorithms.Ranking;

namespace GraphSharp.Graphs;

/// <summary>
/// Page rank algorithm results
/// </summary>
public class PageRankResult{
    /// <summary>
    /// Ranks scores
    /// </summary>
    public IDictionary<int,double> Ranks{get;init;}
    /// <summary>
    /// </summary>
    public PageRankResult(IDictionary<int, double> ranks)
    {
        Ranks = ranks;
    }

    /// <summary>
    /// Algorithm iterations
    /// </summary>
    public int Iterations{get;init;}
    /// <summary>
    /// Maximum difference between scores on last iteration.
    /// </summary>
    public double Precision{get;init;}
}

/// <summary>
/// Page rank algorithm
/// </summary>
public class PageRank<TNode, TEdge> : ImmutableAlgorithmBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// </summary>
    public PageRank(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges) : base(nodes, edges)
    {
    }
    /// <summary>
    /// Computes page rank algorithm
    /// </summary>
    /// <param name="dumping">Dumping factor</param>
    /// <param name="tolerance">Tolerance</param>
    /// <param name="maxIterations">Max number of iterations</param>
    public PageRankResult Compute(double dumping = 0.85, double tolerance = 0.001, int maxIterations = int.MaxValue)
    {
        var score = new ConcurrentDictionary<int, double>();
        var newScore = new ConcurrentDictionary<int, double>();

        var initScore = 1.0/Nodes.Count();
        foreach (var n in Nodes){
            score[n.Id] = initScore;
            newScore[n.Id]=double.MaxValue;
        }

        var nodesArray = Nodes.ToArray();

        var oneMinusDump= 1 - dumping;
        var localDiffs = new ConcurrentBag<double>();

        var iterations= 0 ;
        var maxLocalDiff=double.MaxValue;
        while (maxIterations-- > 0)
        {
            iterations++;
            localDiffs.Clear();
            Parallel.ForEach(nodesArray, n =>
            {
                // newScore[n.Id]=(1-dumping)/nodesArray.Length+dumping*
                var inEdges = Edges.InEdges(n.Id);
                var sum = 0.0;
                foreach (var e in inEdges)
                {
                    var inN = e.SourceId;
                    var outCount = Edges.OutEdges(inN).Count();
                    sum += score[inN] / outCount;
                }
                var newScoreValue = oneMinusDump + dumping * sum;
                newScore[n.Id] = newScoreValue;
                var oldScoreValue = score[n.Id];

                var localDiff = Math.Abs(oldScoreValue - newScoreValue);
                localDiffs.Add(localDiff);
            });

            maxLocalDiff = localDiffs.Max();
            (score,newScore)=(newScore,score);
            if(maxLocalDiff<tolerance) break;
        }
        return new(score){
            Iterations=iterations,
            Precision=maxLocalDiff
        };
    }
}

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Uses custom implementation to compute page rank algorithm.<br/>
    /// It produces same results as quik graph page rank but faster
    /// </summary>
    /// <param name="damping">
    /// Damping factor of page rank. 
    /// In other words how likely that visitor from node A will go to edge A->B. 
    /// So <see langword="1-damping"/> equals to probability that visitor from node A will jump
    /// to any other random node C, that can be adjacent to A, or not.
    /// </param>
    /// <param name="tolerance">
    /// What precision needs to be achieved.
    /// </param>
    /// <param name="maxIterations">
    /// Max amount of iterations of algorithm to run
    /// </param>
    public PageRankResult PageRank(double damping = 0.85, double tolerance = 0.001,int maxIterations = int.MaxValue)
    {
        var pageranks = new PageRank<TNode,TEdge>(Nodes,Edges);
        return pageranks.Compute(damping,tolerance,maxIterations);
    }
    /// <summary>
    /// Uses quik graph implementation to find page rank of current graph.
    /// </summary>
    /// <param name="damping">
    /// Damping factor of page rank. 
    /// In other words how likely that visitor from node A will go to edge A->B. 
    /// So <see langword="1-damping"/> equals to probability that visitor from node A will jump
    /// to any other random node C, that can be adjacent to A, or not.
    /// </param>
    /// <param name="tolerance">
    /// What precision needs to be achieved.
    /// </param>
    public PageRankAlgorithm<int, EdgeAdapter<TEdge>> PageRankQuikGraph(double damping = 0.85, double tolerance = 0.001)
    {
        var quikGraph = StructureBase.Converter.ToQuikGraph();
        var pageRank = new QuikGraph.Algorithms.Ranking.PageRankAlgorithm<int, EdgeAdapter<TEdge>>(quikGraph);
        pageRank.Damping = damping;
        pageRank.Tolerance = tolerance;
        pageRank.Compute();
        return pageRank;
    }
}