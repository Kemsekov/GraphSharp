using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Averaged local clustering coefficients.
    /// </summary>
    /// <param name="clusteringCoefficients">Precomputed clustering coefficients</param>
    /// <returns>Array, where index is node Id and value is coefficient. -1 means this node was not present in the graph.</returns>
    public double[] FindAveragedLocalClusteringCoefficients(double[] clusteringCoefficients)
    {
        var avgClustering = clusteringCoefficients.ToArray();

        Parallel.ForEach(Nodes, n =>
        {
            var neigh = Edges.Neighbors(n.Id).ToList();
            var oldAvg = clusteringCoefficients[n.Id];
            var avg = oldAvg;
            foreach (var nei in neigh)
            {
                avg += clusteringCoefficients[nei];
            }
            avg /= neigh.Count + 1;
            avgClustering[n.Id] = avg;
        });
        return avgClustering;
    }
    /// <summary>
    /// Finds local clustering coefficients
    /// </summary>
    /// <returns>Array, where index is node Id and value is coefficient. -1 means this node was not present in the graph.</returns>
    public RentedArray<double> FindLocalClusteringCoefficients()
    {
        //for each node n take it's neighbors
        //induce graph on {n+neighborhoods} in sum N nodes
        //in induced graph find count of edges = K
        //set coeff for n equal K/(N(N-1))

        var coeff = ArrayPoolStorage.RentArray<double>(Nodes.MaxNodeId + 1);
        coeff.Fill(-1f);
        Parallel.ForEach(Nodes, n =>
        {
            var toInduce = Edges.Neighbors(n.Id).Append(n.Id).ToArray();
            if (toInduce.Length == 1)
            {
                coeff[n.Id] = 0;
                return;
            }
            var induced = Induce(toInduce);
            double N = toInduce.Length;
            double K = induced.Edges.Count;
            coeff[n.Id] = K / (N * (N - 1));
        });
        return coeff;
    }
}