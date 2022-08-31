using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Finds local clustering coefficients
    /// </summary>
    /// <returns>Array, where index is node Id and value is coefficient. -1 means this node was not present in the graph.</returns>
    public float[] FindLocalClusteringCoefficients()
    {
        //for each node n take it's neighbors
        //induce graph on {n+neighborhoods} in sum N nodes
        //in induced graph find count of edges = K
        //set coeff for n equal K/(N(N-1))

        var coeff = new float[Nodes.MaxNodeId+1];
        Array.Fill(coeff,-1f);
        Parallel.ForEach(Nodes,n=>{
            var toInduce = Edges.Neighbors(n.Id).Append(n.Id).ToArray();
            if(toInduce.Length==1){
                coeff[n.Id] = 0;
                return;
            }
            var induced = _structureBase.Do.Induce(toInduce);
            float N = toInduce.Length;
            float K = induced.Edges.Count;
            coeff[n.Id] = K/(N*(N-1));
        });
        return coeff;
    }
}