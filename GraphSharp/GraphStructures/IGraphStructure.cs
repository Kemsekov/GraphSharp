using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public interface IGraphStructure
    {
        /// <summary>
        /// group of nodes that selected to be modified in next invocations.<br/>
        /// For example <see cref="GraphStructure.ForEach"/> will set this property just to <see cref="GraphStructure.Nodes"/> and
        /// next invocations of any operation will be performed on all nodes.
        /// <see cref="GraphStructure.ForOne"/> will set this property to just one particular node from <see cref="GraphStructure.Nodes"/>.
        /// <see cref="GraphStructure.ForNodes"/> will set this property to any subset of <see cref="GraphStructure.Nodes"/> 
        /// </summary>
        /// <value></value>
        IEnumerable<INode> WorkingGroup { get; }
        IList<INode> Nodes { get; }
        Random Rand { get; init; }
        Func<int, INode> CreateNode { get; init; }
        /// <summary>
        /// (node,parent)=>new Edge...
        /// </summary>
        Func<INode, INode, IEdge> CreateEdge { get; init; }
    }
}