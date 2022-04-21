using System;
namespace GraphSharp.Nodes
{
    public interface INode : IComparable<INode>
    {
        /// <summary>
        /// Unique identifier for node
        /// </summary>
        int Id{get;}
        int IComparable<INode>.CompareTo(INode other){
            return this.Id-other.Id;
        }

    }
}