using System;
using System.Linq;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    // TODO: add tests for FindLocalClusteringCoefficients
    /// <summary>
    /// Finds local clustering coefficients
    /// </summary>
    /// <returns>Array, where index is node Id and value is coefficient. -1 means this node was not present in the graph.</returns>
    public float[] FindLocalClusteringCoefficients()
    {
        var coeff = new float[Nodes.MaxNodeId+1];
        Array.Fill(coeff,-1f);
        var neighborhood = new byte[Nodes.MaxNodeId+1];
        foreach(var n in Nodes){
            var edges = Edges.OutEdges(n.Id);
            var edgesCount = edges.Count();
            if(edgesCount<2) continue;
            Array.Fill(neighborhood,(byte)0);
            neighborhood[n.Id] = 1;
            int connectionsCount = edges.Count();
            foreach(var e in edges){
                neighborhood[e.TargetId] = 1;
            }
            foreach(var e in edges){
                foreach(var e1 in Edges.OutEdges(e.TargetId)){
                    if(neighborhood[e1.TargetId]==1) connectionsCount++;
                }
            }
            coeff[n.Id] = ((float)connectionsCount)/(edgesCount*(edgesCount-1));
        }
        return coeff;
    }
}