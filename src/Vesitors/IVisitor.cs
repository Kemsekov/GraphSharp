using System;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// Interface for creating custom Visitors
    /// </summary>
    public interface IVisitor
    {
        
        /// <summary>
        /// Visit node
        /// </summary>
        /// <param name="node">node to visit</param>
        void Visit(NodeBase node);
        /// <summary>
        /// End visit for node
        /// </summary>
        /// <param name="node">node to be end visited</param>
        /// <param name="visited">Whatever current visitor already end visited node or not</param>
        void EndVisit(NodeBase node);
        /// <summary>
        /// Method that selects which nodes need to be visited and which not
        /// </summary>
        /// <param name="node">Node to be selected</param>
        /// <returns>True - visit node. False - not visit node</returns>
        bool Select(NodeBase node);
    }
}