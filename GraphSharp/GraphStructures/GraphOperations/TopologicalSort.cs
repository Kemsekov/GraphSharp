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
    /// Do topological sort on the graph.
    /// </summary>
    /// <returns>Object that contains List of layers where each layer is a list of nodes at a certain layer of topological sort. Use <see cref="TopologicalSorter{,}.DoTopologicalSort"/> to change X coordinates to a certain value.</returns>
    public TopologicalSorter<TNode,TEdge> TopologicalSort()
    {
        var alg = new TopologicalSorter<TNode, TEdge>(_structureBase);
        while (!alg.Done)
        {
            alg.Propagate();
        }
        return alg;
    }
}