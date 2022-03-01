using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            _convertedEdges = new ConvertableList<TEdge,IEdge>(Edges);
        }
        public int Id{get;init;}
        public IList<TEdge> Edges{get;}
        ConvertableList<TEdge, IEdge> _convertedEdges;

        IList<IEdge> INode.Edges => _convertedEdges;
    }
}