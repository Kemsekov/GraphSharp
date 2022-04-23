using System.Collections.Generic;
using GraphSharp.Edges;
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
        }
        public int Id{get;set;}
        public IList<TEdge> Edges{get;}
    }
}