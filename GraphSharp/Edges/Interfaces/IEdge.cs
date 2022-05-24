using System;
using GraphSharp.Nodes;
namespace GraphSharp.Edges
{
    public interface IEdge : IComparable<IEdge>
    {
        INode Source{get;}
        INode Target{get;}
        int IComparable<IEdge>.CompareTo(IEdge? other){
            if(other==null)
                return 1;
            return this.GetHashCode()-other.GetHashCode();
        }
    }
}