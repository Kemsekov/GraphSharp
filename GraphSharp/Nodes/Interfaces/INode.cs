using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;

namespace GraphSharp.Nodes
{
    public interface INode : IComparable<INode>
    {
        /// <summary>
        /// Unique identifier for node
        /// </summary>
        int Id{get;}
        /// <summary>
        /// Edges of a current node.
        /// </summary>
        IEnumerable<IEdge> Edges{get;}
        int IComparable<INode>.CompareTo(INode other){
            return this.Id-other.Id;
        }

    }
}