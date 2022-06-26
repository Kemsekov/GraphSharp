using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
namespace GraphSharp.Propagators
{
    public interface IPropagator<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        void Propagate();
        void SetPosition(params int[] nodeIndices);
        void SetGraph(IGraphStructure<TNode,TEdge> graph);
        public bool IsNodeInState(int nodeId, byte state);
        public void SetNodeState(int nodeId, byte state);
        public void RemoveNodeState(int nodeId, byte state);
    }
}