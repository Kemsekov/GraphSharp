using GraphSharp.Edges;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
namespace GraphSharp.Propagators
{
    public interface IPropagator<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        void Propagate();
        void SetPosition(params int[] nodeIndices);
        void SetGraph(IGraph<TNode,TEdge> graph);
        public bool IsNodeInState(int nodeId, byte state);
        public void SetNodeState(int nodeId, byte state);
        public void RemoveNodeState(int nodeId, byte state);
        public byte GetNodeStates(int nodeId);
        public void ClearNodeStates(int nodeId);
    }
}