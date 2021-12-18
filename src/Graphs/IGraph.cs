using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Implements logic for propagating visitors trough graph
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
        /// Propagate visitor trough graph one generation -> from current nodes to their children
        /// </summary>
        /// <param name="visitor"></param>
        void Step(IVisitor visitor);
        /// <summary>
        /// Propagate all visitors trough graph one generation -> from current nodes to their children
        /// </summary>
        void Step();
    }
}