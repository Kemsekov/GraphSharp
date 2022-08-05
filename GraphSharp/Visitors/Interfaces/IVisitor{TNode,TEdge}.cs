

namespace GraphSharp.Visitors
{
    public interface IVisitor<TNode, TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        bool Select(TEdge edge);
        void Visit(TNode node);
        void EndVisit();        
    }
}