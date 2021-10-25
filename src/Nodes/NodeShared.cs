using System;
using System.Collections.Generic;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Abstract class that contain shared logic between all nodes
    /// </summary>
    /// <typeparam name="TChild">The type inherited from <see cref="IChild"/> that contain all data about every child of this node</typeparam>
    public abstract class NodeShared<TChild> : INode where TChild : IChild
    {
        /// <summary>
        /// Id of current node. Must be unique in collections of nodes.
        /// </summary>
        /// <value></value>
        public int Id { get; }
        /// <summary>
        /// Children of current node
        /// </summary>
        /// <typeparam name="TChild"></typeparam>
        /// <returns></returns>
        public List<TChild> Children{get;} = new List<TChild>();
        public NodeShared(int id)
        {
            Id = id;
        }
        /// <summary>
        /// Adds node of <see cref="TNode"/> as child independently of what <see cref="TChild"/> is.
        /// </summary>
        /// <param name="node">Node to be added</param>
        /// <typeparam name="TNode">The type of node</typeparam>
        public abstract void AddChild<TNode>(TNode node) where TNode : NodeShared<TChild>;
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"Node : {Id}";
        }

        public int CompareTo(INode other)
        {
            return Id - other.Id;
        }
    }
}