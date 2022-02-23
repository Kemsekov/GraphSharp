using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Edges;

namespace GraphSharp.Nodes
{
    public interface INode<TEdge> : INode
    where TEdge : IEdge
    {
        /// <summary>
        /// Edges of a current node.
        /// </summary>
        new IList<TEdge> Edges{get;}
        IEnumerable<IEdge> INode.Edges => this.Edges.Select(n=>n as IEdge);
    }
}