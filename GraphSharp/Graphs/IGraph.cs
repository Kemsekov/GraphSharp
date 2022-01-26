using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Manage a bunch of <see cref="IPropagator"/>s mostly by hiding them. Exposing simpler interface. 
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// adds visitor to this graph and assign visitor to some one random node from graph.
        /// </summary>
        void AddVisitor(IVisitor visitor);
        /// <summary>
        /// adds visitor to this graph and bind it to nodes with id equal to indices
        /// </summary>
        void AddVisitor(IVisitor visitor,params int[] indices);
        void RemoveVisitor(IVisitor visitor);
        void RemoveAllVisitors();
        /// <summary>
        /// Propagate visitor trough graph by one generation -> from current nodes to their children by edges
        /// </summary>
        /// <param name="visitor"></param>
        void Propagate(IVisitor visitor);
        /// <summary>
        /// Propagate all visitors trough graph one generation -> from current nodes to their children by edges
        /// </summary>
        void Propagate();
    }
}