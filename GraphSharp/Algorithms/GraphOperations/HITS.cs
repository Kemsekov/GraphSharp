using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Adapters;
using GraphSharp.Algorithms;
using GraphSharp.Exceptions;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Ranking;

namespace GraphSharp.Graphs;

/// <summary>
/// HITS algorithm results
/// </summary>
public class HITSResults{

    /// <summary>
    /// Authorities scores
    /// </summary>
    public IDictionary<int,double> AuthScores{get;init;}
    /// <summary>
    /// </summary>
    public HITSResults(IDictionary<int, double> authScores,IDictionary<int, double> hubScores)
    {
        AuthScores = authScores;
        HubScores=hubScores;
    }

    /// <summary>
    /// Hub scores
    /// </summary>
    public IDictionary<int,double> HubScores{get;init;}
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
/// Hubs and authorities algorithm
/// </summary>
public class HITS<TNode, TEdge> : ImmutableAlgorithmBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// </summary>
    public HITS(IImmutableNodeSource<TNode> nodes, IImmutableEdgeSource<TEdge> edges) : base(nodes, edges)
    {
    }
    /// <summary>
    /// HITS algorithm computation
    /// </summary>
    /// <param name="rootSet">
    /// Root set of HITS algorithm. It is a subset of points of graph that is considered to be trustworthy.
    /// </param>
    /// <param name="tolerance">
    /// What precision needs to be achieved.
    /// </param>
    /// <param name="maxIterations">
    /// Max amount of iterations of algorithm.
    /// </param>
    public HITSResults Compute(int[] rootSet, double tolerance = 0.01,int maxIterations = int.MaxValue){
        if(rootSet.Any(i=>!Nodes.Contains(i)))
            throw new NodeNotFoundException("provided root set contains node ids that are not found in a graph");
        var baseSet = 
            rootSet
            .AsParallel()
            .SelectMany(
                node=>
                Edges
                .OutEdges(node)
                .Concat(Edges.InEdges(node))
                .SelectMany(e=>new[]{e.SourceId,e.TargetId})
            )
            .Concat(rootSet.AsParallel())
            .Distinct()
            .ToList();
        
        var hubScores  = new ConcurrentDictionary<int,double>();
        var authScores = new ConcurrentDictionary<int,double>();
        var newHubScores  = new ConcurrentDictionary<int,double>();
        var newAuthScores = new ConcurrentDictionary<int,double>();


        var nodesArray = Nodes.ToArray();
        
        foreach(var n in nodesArray){
            hubScores[n.Id]=authScores[n.Id]=0;
            newHubScores[n.Id]=newAuthScores[n.Id]=0;
        }
        foreach(var n in baseSet){
            hubScores[n]=authScores[n]=1;
        }

        var localDiffs = new List<double>(nodesArray.Length);
        var iterations= 0 ;
        var maxLocalDiff=0.0;
        var hubLength=1.0;
        var authLength=1.0;
        while(maxIterations-->0){
            iterations++;
            localDiffs.Clear();
            Parallel.ForEach(nodesArray,n=>{
                var outEdges = Edges.OutEdges(n.Id);
                var inEdges = Edges.InEdges(n.Id);

                var newAuth = 0.0;
                foreach(var e in inEdges){
                    var hub = e.SourceId;
                    var score = hubScores[hub];
                    newAuth+=score;
                }

                var newHub = 0.0;
                foreach(var e in outEdges){
                    var auth = e.TargetId;
                    var score = authScores[auth];
                    newHub+=score;
                }

                newHubScores[n.Id]=newHub;
                newAuthScores[n.Id]=newAuth;

                var oldHub = hubScores[n.Id];
                var oldAuth = authScores[n.Id];

                var maxLocalDiff = Math.Max(Math.Abs(newHub/hubLength-oldHub),Math.Abs(newAuth/authLength-oldAuth));
                lock(localDiffs)
                    localDiffs.Add(maxLocalDiff);
            });

            hubLength = Math.Sqrt(newHubScores.Values.Sum(v=>v*v));
            authLength = Math.Sqrt(newAuthScores.Values.Sum(v=>v*v));

            foreach(var n in nodesArray){
                newHubScores[n.Id]/=hubLength;
                newAuthScores[n.Id]/=authLength;
            }

            maxLocalDiff = localDiffs.Max();
            (hubScores,newHubScores)=(newHubScores,hubScores);
            (authScores,newAuthScores)=(newAuthScores,authScores);
            if(maxLocalDiff<tolerance) break;
        }
        return new(authScores,hubScores){
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
    /// HITS algorithm implementation
    /// </summary>
    /// <param name="rootSet">
    /// Root set of HITS algorithm. It is a subset of points of graph that is considered to be trustworthy.
    /// </param>
    /// <param name="tolerance">
    /// What precision needs to be achieved.
    /// </param>
    /// <param name="maxIterations">
    /// Max amount of iterations of algorithm
    /// </param>
    public HITSResults HITS(int[] rootSet, double tolerance = 0.01,int maxIterations = int.MaxValue)
    {
        var hits = new HITS<TNode,TEdge>(Nodes,Edges);
        return hits.Compute(rootSet,tolerance);
    }
}