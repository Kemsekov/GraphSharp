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
        /// <summary>
        /// Node of current edge. Represent connection between parent and node.
        /// </summary>
        public TNode Child{get;set;}
        /// <summary>
        /// Parent of current edge.
        /// </summary>
        public TNode Parent{get;set;}
        INode IEdge.Child=>this.Child;
        INode IEdge.Parent=>this.Parent;
    }
}