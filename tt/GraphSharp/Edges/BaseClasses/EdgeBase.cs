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
        public EdgeBase(TNode source, TNode target)
        {
            Source = source;
            Target = target;
        }
        public virtual TNode Source{get;set;}
        public virtual TNode Target{get;set;}
        INode IEdge.Source=>this.Source;
        INode IEdge.Target=>this.Target;
    }
}