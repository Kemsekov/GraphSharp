using GraphSharp.Graphs;

namespace GraphSharp.Algorithms;
public abstract class AlgorithmBase<TNode,TEdge>
where TNode : INode
where TEdge : IEdge
{
    public AlgorithmBase(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges)
    {
        Nodes = nodes;
        Edges = edges;
    }
    public INodeSource<TNode> Nodes { get; }
    public IEdgeSource<TEdge> Edges { get; }
}