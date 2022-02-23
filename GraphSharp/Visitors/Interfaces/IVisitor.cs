using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Propagators;

namespace GraphSharp.Visitors
{

    public interface IVisitor
    {
        bool Select(IEdge edge);
        void Visit(INode node);
        void EndVisit();
    }
}