using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphSharp.Visitors;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base class for all nodes
    /// </summary>
    public abstract class NodeBase : IComparable<NodeBase>
    {
        /// <summary>
        /// Childs of current node
        /// </summary>
        /// <value></value>
        public abstract List<NodeBase> Childs{get;}
        /// <summary>
        /// Id of current node. Must be unique in collections of nodes.
        /// </summary>
        /// <value></value>
        public int Id { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Node id. Must be unique in collections of nodes</param>
        public NodeBase(int id)
        {
            Id = id;
        }
        /// <summary>
        /// Adds child to current node.
        /// </summary>
        /// <param name="child">Node to be added as child of this node.</param>
        public void AddChild(NodeBase child)
        {
            Childs.Add(child);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"Node : {Id}";
        }
        public int CompareTo(NodeBase other)=>Id-other.Id;
    }
}