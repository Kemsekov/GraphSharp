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
        /// <summary>
        /// A subset of <see cref="IGraphStructure{}.Nodes"/> that selected to be modified.<br/>
        /// </summary>
        /// <value></value>
        IEnumerable<TNode> WorkingGroup { get; }
        IList<TNode> Nodes { get; }

    }
}