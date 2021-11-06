using System;
using GraphSharp.Children;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Interface for creating custom Visitors.
    /// </summary>
    public interface IVisitor
    {
        bool Select(IChild node);
        void Visit(IChild node);
        void EndVisit();
    }
}