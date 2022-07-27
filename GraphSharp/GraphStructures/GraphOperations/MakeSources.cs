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
    /// Will create sources on nodes with id equal to nodeIndices. <br/>
    /// In other words after this method used any possible path in a graph
    /// will land on one of the nodes you specified. <br/>
    /// </summary>
    /// <param name="nodeIndices"></param>
    public GraphOperation<TNode, TEdge> MakeSources(params int[] nodeIndices)
    {
        if (nodeIndices.Count() == 0 || _structureBase.Nodes.Count == 0) return this;

        var Nodes = _structureBase.Nodes;
        var Edges = _structureBase.Edges;
        foreach (var i in nodeIndices)
            if (i > Nodes.MaxNodeId)
                throw new ArgumentException("nodeIndex is out of range");
        var sourceCreator = new SourceCreator<TNode, TEdge>(_structureBase);

        foreach (var n in nodeIndices)
        {
            foreach (var source in Edges.GetSourcesId(n).ToArray())
            {
                Edges.Remove(source, n);
            }
        }

        sourceCreator.SetPosition(nodeIndices);
        while (sourceCreator.DidSomething)
        {
            sourceCreator.DidSomething = false;
            sourceCreator.Propagate();
        }
        return this;

    }
}