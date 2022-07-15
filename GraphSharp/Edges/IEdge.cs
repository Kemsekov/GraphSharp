using System;
using GraphSharp.Common;
using GraphSharp.Nodes;
namespace GraphSharp.Edges
{
    public interface IEdge<TNode> : IComparable<IEdge<TNode>>, IColored, IWeighted, IFlowed
    {
        TNode Source{get;set;}
        TNode Target{get;set;}
        int IComparable<IEdge<TNode>>.CompareTo(IEdge<TNode>? other){
            if(other==null)
                return 1;
            return this.GetHashCode()-other.GetHashCode();
        }
    }
}