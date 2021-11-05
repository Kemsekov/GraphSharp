using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    public interface IGraph
    {
        void AddVisitor(IVisitor visitor);
        void AddVisitor(IVisitor visitor,params int[] indices);
        void RemoveVisitor(IVisitor visitor);
        void RemoveAllVisitors();
        void Step(IVisitor visitor);
        void Step();
    }
}