using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Extensions;

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