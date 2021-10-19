using System;
using GraphSharp.Nodes;

namespace GraphSharp.Vesitos
{
    public interface IVesitor : IComparable<IVesitor>
    {
        
        /// <summary>
        /// Vesit node
        /// </summary>
        /// <param name="node">node to vesit</param>
        /// <param name="vesited">Whatever current vesitor already vesited node</param>
        void Vesit(NodeBase node, bool vesited = false);
        /// <summary>
        /// End vesit for node
        /// </summary>
        /// <param name="node">node to be end vesited</param>
        /// <param name="vesited">Whatever current vesitor already end vesited node or not</param>
        void EndVesit(NodeBase node, bool vesited = false);
        /// <summary>
        /// Method that selects which nodes need to be vesited and which not
        /// </summary>
        /// <param name="node">Node to be selected</param>
        /// <returns>True - vesit node. False - not vesit node</returns>
        bool Select(NodeBase node);
    }
}