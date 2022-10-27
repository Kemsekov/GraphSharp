using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;

namespace GraphSharp.Algorithms;
public abstract class ColoringAlgorithmBase<TNode,TEdge>
where TNode : INode
where TEdge : IEdge
{
    public ColoringAlgorithmBase(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges)
    {
        Nodes = nodes;
        Edges = edges;
    }
    public INodeSource<TNode> Nodes { get; }
    public IEdgeSource<TEdge> Edges { get; }
}