using System;
using System.Collections.Generic;

namespace GraphSharp.Nodes
{
    public abstract class NodeShared<TChild> : INode where TChild : IChild
    {
        /// <summary>
        /// Id of current node. Must be unique in collections of nodes.
        /// </summary>
        /// <value></value>
        public int Id { get; }
        public List<TChild> Children{get;} = new List<TChild>();
        public NodeShared(int id)
        {
            Id = id;
        }
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