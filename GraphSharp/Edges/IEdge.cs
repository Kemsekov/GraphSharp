using System;
using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    /// <summary>
    /// Edge of node
    /// </summary>
    public interface IEdge : IComparable<IEdge>
    {
        INode Node{get;init;}
        int IComparable<IEdge>.CompareTo(IEdge other){
            return this.GetHashCode()-other.GetHashCode();
        }
    }
}