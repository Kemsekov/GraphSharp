using GraphSharp.Nodes;
namespace GraphSharp.Edges
{
    /// <summary>
    /// Base edge class
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public abstract class EdgeBase<TNode> : IEdge
    where TNode : INode
    {
        public EdgeBase(TNode parent, TNode child)
        {
            Child = child;
            Parent = parent;
        }
        public virtual TNode Child{get;set;}
        public virtual TNode Parent{get;set;}
        INode IEdge.Child=>this.Child;
        INode IEdge.Parent=>this.Parent;
    }
}