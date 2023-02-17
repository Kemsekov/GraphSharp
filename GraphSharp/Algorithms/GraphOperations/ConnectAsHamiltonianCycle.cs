using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Connects graph nodes as hamiltonian cycle
    /// </summary>
    public GraphOperation<TNode, TEdge> ConnectAsHamiltonianCycle(Func<TNode,Vector> getPos)
    {
        var tsp = TspCheapestLinkOnPositions(getPos);
        tsp = TspOpt2(tsp.Tour,tsp.TourCost,(n1,n2)=>(getPos(n1)-getPos(n2)).L2Norm());
        tsp.Tour.Aggregate((n1,n2)=>{
            Edges.Add(Configuration.CreateEdge(n1,n2));
            return n2;
        });
        return this;
    }
}