using System;
using GraphSharp.Nodes;
namespace GraphSharp.Edges
{
    public interface IEdge : IComparable<IEdge>
    {
        INode Child{get;}
        INode Parent{get;}
        int IComparable<IEdge>.CompareTo(IEdge other){
            return this.GetHashCode()-other.GetHashCode();
        }
    }
}