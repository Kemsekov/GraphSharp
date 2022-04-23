using GraphSharp.Edges;
using GraphSharp.Nodes;
namespace GraphSharp.Visitors
{
    public interface IVisitor<TNode, TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        bool Select(TEdge edge);
        void Visit(TNode node);
        void EndVisit();        
    }
}