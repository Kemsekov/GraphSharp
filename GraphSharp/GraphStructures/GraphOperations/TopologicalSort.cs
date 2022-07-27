using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : Edges.IEdge<TNode>
{
    /// <summary>
    /// Do topological sort on the graph. Changes X coordinates of nodes so any following nodes are ancestors of previous once and have bigger X coordinate
    /// </summary>
    public void TopologicalSort()
    {
        var alg = new TopologicalSorter<TNode, TEdge>(_structureBase);
        while (!alg.Done)
        {
            alg.Propagate();
        }
        alg.DoTopologicalSort();
    }
}