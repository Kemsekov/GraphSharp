using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
namespace GraphSharp.Propagators
{
    public interface IPropagator<TNode>
    where TNode : INode
    {
        void Propagate();
        void SetPosition(params int[] nodeIndices);
        void SetNodes(IGraphStructure<TNode> nodes);
    }
}