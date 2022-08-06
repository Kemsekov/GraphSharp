using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GraphSharp.Visitors;

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
        var coeff = new float[Nodes.MaxNodeId+1];
        Array.Fill(coeff,-1f);
        var neighbourhood = new byte[Nodes.MaxNodeId+1];
        foreach(var n in Nodes){
            var edges = Edges[n.Id];
            var edgesCount = edges.Count();
            if(edgesCount<2) continue;
            Array.Fill(neighbourhood,(byte)0);
            neighbourhood[n.Id] = 1;
            int connectionsCount = edges.Count();
            foreach(var e in edges){
                neighbourhood[e.TargetId] = 1;
            }
            foreach(var e in edges){
                foreach(var e1 in Edges[e.TargetId]){
                    if(neighbourhood[e1.TargetId]==1) connectionsCount++;
                }
            }
            coeff[n.Id] = ((float)connectionsCount)/(edgesCount*(edgesCount-1));
        }
        return coeff;
    }
}