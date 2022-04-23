using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Graph structure holder.
    /// </summary>
    public interface IGraphStructure<TNode>
    where TNode : INode
    {
        IList<TNode> Nodes { get; }
    }
}