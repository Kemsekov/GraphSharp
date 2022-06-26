using GraphSharp.Edges;
using GraphSharp.Nodes;
namespace GraphSharp.Visitors
{
    public interface IVisitor<TNode, TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        bool Select(TEdge edge);
        void Visit(TNode node);
        void EndVisit();        
    }
}