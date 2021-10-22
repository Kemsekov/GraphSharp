using System;
using GraphSharp.Nodes;

namespace GraphSharp
{
    public interface IChild : IComparable<IChild>
    {
        INode NodeBase{get;}
    }
}