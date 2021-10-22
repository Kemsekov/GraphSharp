using System;

namespace GraphSharp.Nodes
{
    public interface INode : IComparable<INode>
    {
        int Id{get;}
    }
}