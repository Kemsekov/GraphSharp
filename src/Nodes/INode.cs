using System;
using System.Collections.Generic;
using GraphSharp.Children;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base interface for all nodes
    /// </summary>
    public interface INode : IComparable<INode>
    {
        int Id{get;}
        IList<IChild> Children{get;}
    }
}