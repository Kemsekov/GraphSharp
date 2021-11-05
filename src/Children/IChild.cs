using System;
using GraphSharp.Nodes;

namespace GraphSharp.Children
{
    public interface IChild : IComparable<IChild>
    {
        INode Node{get;}
    }
}