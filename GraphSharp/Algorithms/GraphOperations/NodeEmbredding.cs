using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{

    /// <summary>
    /// Creates nodes embeddings that represents node unique position relative to all other nodes and edges in a graph.<br/>
    /// Can be used to determine if two graphs are isomorphic.
    /// </summary>
    /// <param name="tolerance">
    /// Tolerance to node embeddings. Varying these greatly changes execution time
    /// </param>
    /// <param name="maxIterations">Max iterations of embedding algorithms to run</param>
    /// <returns>Embeddings nodeId -> vector</returns>
    public IDictionary<int, double[]> NodesEmbedding(double tolerance = 0.01,int maxIterations=100)
    {
        //here we use different structural information about nodes to give them unique embeddings.
        //This setup may differ, here I use pagerank, HITS and local clustering coefficients
        //this setup works +- well from big graphs up to 5000

        var pagerank1 = StructureBase.Do.PageRank(0.95,tolerance,maxIterations).Ranks;
        var pagerank2 = StructureBase.Do.PageRank(0.75,tolerance,maxIterations).Ranks;

        var mostImportantNodes1 = 
            pagerank1.Keys.Zip(
                pagerank1.Keys
                    .Select(k => pagerank1[k]))
            .OrderBy(v => -v.Second).ToArray();
        // var mostImportantNodes2 =
        //     pagerank2.Keys.Zip(
        //         pagerank1.Keys
        //             .Select(k => pagerank2[k]))
        //     .OrderBy(v => v.Second).ToArray();

        var rootSet1=
            mostImportantNodes1
            .Take(Math.Max(StructureBase.Nodes.Count() / 4, 1))
            .Select(i => i.First).ToArray();
        
        // var rootSet2=
        //     mostImportantNodes2
        //     .Take(Math.Max(StructureBase.Nodes.Count() / 4, 1))
        //     .Select(i => i.First).ToArray();

        
        var HITS1 =StructureBase.Do.HITS(rootSet1, tolerance,maxIterations);
        // var HITS2 =StructureBase.Do.HITS(rootSet2, tolerance,maxIterations);

        var auth1 = HITS1.AuthScores;
        // var auth2 = HITS2.AuthScores;
        var hub1 =  HITS1.HubScores;
        // var hub2 =  HITS2.HubScores;

        using var clustering = FindLocalClusteringCoefficients();


        //another fun set of metric that seems to work well
        var avgClustering = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>clustering[i]);
        var avgAuth = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>auth1[i]);
        var avgHub = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>hub1[i]);
        var avgPageRank = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>pagerank1[i]+pagerank2[i]);
        var avgSumOfAll = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>pagerank1[i]+pagerank2[i]+auth1[i]+clustering[i]+hub1[i]);
        

        var nodeVectors = StructureBase.Nodes.ToDictionary(
            n => n.Id, 
            n => new[] { 
                pagerank1[n.Id], 
                pagerank2[n.Id], 
                auth1[n.Id], 
                // auth2[n.Id], 
                hub1[n.Id],
                // hub2[n.Id],
                clustering[n.Id],
                avgClustering[n.Id],
                avgAuth[n.Id],
                avgHub[n.Id],
                avgPageRank[n.Id],
                avgSumOfAll[n.Id]
            });
        var max = nodeVectors.First().Value.ToArray();
        var min = nodeVectors.First().Value.ToArray();
        
        Array.Fill(max,double.MinValue);
        Array.Fill(min,double.MaxValue);

        var dims = max.Length;
        foreach(var n in Nodes){
            var value = nodeVectors[n.Id];
            for(int i = 0;i<dims;i++){
                max[i]=Math.Max(max[i],value[i]);
                min[i]=Math.Min(min[i],value[i]);
            }
        }

        foreach(var n in Nodes){
            var value = nodeVectors[n.Id];
            for(int i = 0;i<dims;i++){
                value[i]=(value[i]-min[i])/(max[i]-min[i]);
            }
        }


        return nodeVectors;
    }

}