using System;
using GraphSharp.Common;
using GraphSharp.Nodes;
namespace GraphSharp.Edges
{
    public interface IEdge<TNode> : IComparable<IEdge<TNode>>, IColored, IWeighted, IFlowed
    where TNode : INode
    {
        TNode Source{get;set;}
        TNode Target{get;set;}
        int IComparable<IEdge<TNode>>.CompareTo(IEdge<TNode>? other){
            if(other is null)
                return 1;
            var d1 = Source.Id-other.Source.Id;
            var d2 = Target.Id-other.Target.Id;
            if(d1==0) return d2;
            return d1;
        }
    }
}