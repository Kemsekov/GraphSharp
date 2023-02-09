using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.Graphs;

// TODO: add test

public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    ///<summary>
    /// Computes minimal not intersecting max cliques of graph that contains all nodes
    ///</summary>
    public IEnumerable<CliqueResult> FindMinNotIntersectingMaxCliques()
    {
        var clone = new Graph<TNode,TEdge>(StructureBase.Configuration);
        clone.SetSources(nodes: StructureBase.Nodes, edges: StructureBase.Edges);
        while(true){
            var clique = clone.Do.FindMaxClique();
            if(clique.Nodes.Count>0){
                yield return clique;
                clone.Do.RemoveNodes(clique.Nodes.ToArray());
                continue;
            }
            break;
        }
    }
    ///<summary>
    /// Computes minimal not intersecting max cliques of graph that contains all nodes, using fast max clique search algorithm. <br/>
    /// May produce less optimal results, but a lot faster.
    ///</summary>
    public IEnumerable<CliqueResult> FindMinNotIntersectingMaxCliquesFast()
    {
        var clone = new Graph<TNode,TEdge>(StructureBase.Configuration);
        clone.SetSources(nodes: StructureBase.Nodes, edges: StructureBase.Edges);
        while(true){
            var clique = clone.Do.FindMaxCliqueFast();
            if(clique.Nodes.Count>0){
                yield return clique;
                clone.Do.RemoveNodes(clique.Nodes.ToArray());
                continue;
            }
            break;
        }

    }
}