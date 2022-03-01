using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Extensions;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Base generic <see cref="INode"/> implementation 
    /// </summary>
    /// <typeparam name="TEdge"></typeparam>
    public abstract class NodeBase<TEdge> : INode
    where TEdge : IEdge
    {
        public NodeBase(int id)
        {
            Id = id;
            Edges = new List<TEdge>();
            _convertedEdges = new(Edges);
        }
        public int Id{get;init;}
        public IList<TEdge> Edges{get;}
        ConvertableEnumerable<TEdge,IEdge> _convertedEdges;
        IEnumerable<IEdge> INode.Edges => _convertedEdges;
    }
}