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
    /// <returns>Embeddings nodeId -> vector</returns>
    public IDictionary<int, double[]> NodesEmbedding(double tolerance = 0.01)
    {
        //here we use different structural information about nodes to give them unique embeddings.
        //This setup may differ, here I use pagerank, HITS and local clustering coefficients
        //this setup works +- well from big graphs up to 5000


        var pageranks = new[]{
            StructureBase.Do.PageRank(0.95,tolerance).Ranks,
            StructureBase.Do.PageRank(0.75,tolerance).Ranks,
            // StructureBase.Do.PageRank(0.55,tolerance).Ranks,
        };

        var mostImportantNodes = 
        pageranks
        .Select((pagerank,index)=>
            pagerank.Keys.Zip(
                pagerank.Keys
                    .Select(k => pagerank[k]))
            .OrderBy(v => (index%2==0 ? -1 : 1)*v.Second).ToArray());
        
        var rootSets = mostImportantNodes.Select(
            (mostImportantNodes,index)=>
                mostImportantNodes
                .Take(Math.Max(StructureBase.Nodes.Count() / (index+2), 1))
                .Select(i => i.First).ToArray()
            );
        
        var HITSs = 
            rootSets
            .Select(rootSet => 
                StructureBase.Do.HITS(rootSet, tolerance))
            .ToList();
        var auths = HITSs.Select(c=>c.AuthScores).ToList();
        var hubs =  HITSs.Select(c=>c.HubScores) .ToList();

        using var clustering = FindLocalClusteringCoefficients();
        

        //another fun set of metric that seems to work well
        // var avgClustering = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>clustering[i]);
        // var avgAuth = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>auth[i]);
        // var avgHub = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>hub[i]);
        // var avgPageRank = StructureBase.Do.FindAveragedLocalClusteringCoefficients(i=>pagerank[i]);
        

        var nodeVectors = StructureBase.Nodes.ToDictionary(
            n => n.Id, 
            n => new[] { 
                pageranks[0][n.Id], 
                pageranks[1][n.Id], 
                // pageranks[2][n.Id], 
                auths[0][n.Id], 
                auths[1][n.Id], 
                // auths[2][n.Id], 
                hubs[0][n.Id],
                hubs[1][n.Id],
                // hubs[2][n.Id],
                clustering[n.Id]
            });

        return nodeVectors;
    }

}