using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
namespace GraphSharp.Propagators
{
    public interface IPropagator<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        void Propagate();
        void SetPosition(params int[] nodeIndices);
        void SetGraph(IGraphStructure<TNode,TEdge> graph);
    }
}