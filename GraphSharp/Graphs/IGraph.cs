using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Manage a bunch of <see cref="IPropagator"/>s and <see cref="IVisitor"/>s
    /// </summary>
    public interface IGraph
    {
        void AddVisitor(IVisitor visitor);
        void AddVisitor(IVisitor visitor,params int[] indices);
        void RemoveVisitor(IVisitor visitor);
        void RemoveAllVisitors();
        void Propagate(IVisitor visitor);
        void Propagate();
    }
}