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
    public IDictionary<int, double[]> NodesEmbedding()
    {
        //here we use different structural information about nodes to give them unique embeddings.
        //This setup may differ, here I use pagerank, HITS and local clustering coefficients
        //this setup works +- well from big graphs up to 5000

        IDictionary<int, double> pagerank = new Dictionary<int, double>();
        IDictionary<int, double> auth = new Dictionary<int, double>();
        IDictionary<int, double> hub = new Dictionary<int, double>();

        pagerank = StructureBase.Do.PageRank(0.9).Ranks;
        using var c1 = StructureBase.Do.FindLocalClusteringCoefficients();
        var clustering = c1.ToArray();

        var mostImportantNodes = pagerank.Keys.Zip(pagerank.Keys.Select(k => pagerank[k])).OrderBy(v => -v.Second).ToArray();

        var rootSet = mostImportantNodes.Take(Math.Max(StructureBase.Nodes.Count() / 4, 1)).Select(i => i.First).ToArray();
        var c = StructureBase.Do.HITS(rootSet, 0.01);
        auth = c.AuthScores;
        hub = c.HubScores;

        var nodeVectors = StructureBase.Nodes.ToDictionary(n => n.Id, n => new[] { clustering[n.Id], pagerank[n.Id], auth[n.Id], hub[n.Id] });
        return nodeVectors;
    }

}