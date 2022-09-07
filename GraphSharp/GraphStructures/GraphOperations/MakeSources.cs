using System;
using System.Linq;
using GraphSharp.Visitors;
namespace GraphSharp.Graphs;

public partial class GraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Will create sources on nodes with id equal to nodeIndices. <br/>
    /// In other words after this method used any possible path in a reversed graph
    /// will land on one of the nodes you specified. <br/>
    /// </summary>
    /// <param name="nodeIndices"></param>
    public GraphOperation<TNode, TEdge> MakeSources(params int[] nodeIndices)
    {
        if (nodeIndices.Count() == 0 || Nodes.Count == 0) return this;

        foreach (var i in nodeIndices)
            if (i > Nodes.MaxNodeId)
                throw new ArgumentException("nodeIndex is out of range");
        var sourceCreator = new SourceCreator<TNode, TEdge>(_structureBase);

        foreach (var n in nodeIndices)
        {
            foreach (var inEdge in Edges.InEdges(n).ToArray())
            {
                Edges.Remove(inEdge.SourceId, n);
            }
        }

        sourceCreator.SetPosition(nodeIndices);
        while (!sourceCreator.Done)
        {
            sourceCreator.Propagate();
        }
        return this;

    }
}