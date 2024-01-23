using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Single;
using QuikGraph;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<summary>
    /// Computes distance matrix from graph nodes, using distance metric.
    ///</summary>
    /// <param name="distance">How to compute distances between nodes</param>
    /// <returns>Matrix where each (i,j) element corresponds to distance between node under index i and node under index j</returns>
    public float[,] DistanceMatrix(Func<TNode,TNode,float> distance)
    {
        var nodes = Nodes.ToList();
        var distances = new float[Nodes.MaxNodeId+1,Nodes.MaxNodeId+1];
        Parallel.ForEach(nodes,n1=>{
            foreach(var n2 in nodes){
                distances[n1.Id,n2.Id]=distance(n1,n2);
            }
        });
        return distances;
    }

}