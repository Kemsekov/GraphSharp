using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.Graphs;

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<summary>
    /// Arranges graph and returns graph arrangement
    ///</summary>
    /// <param name="theta">Compute algorithm step until summary or node positions change is bigger that theta</param>
    /// <param name="closestCount">Count of closest to given node elements to compute repulsion from. Let it be -1 so all nodes will be used to compute repulsion</param>
    /// <param name="getWeight">How to measure edge weights. By default will use distance between edge endpoints.</param>
    public GraphArrange<TNode, TEdge> Arrange(float theta, int closestCount = -1, Func<TEdge,float>? getWeight = null)
    {
        var p = new GraphArrange<TNode,TEdge>(StructureBase);
        while(p.ComputeStep()>theta) ;
        return p;
    }

}