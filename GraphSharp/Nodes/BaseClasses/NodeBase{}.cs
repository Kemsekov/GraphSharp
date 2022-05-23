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
        }
        public int Id{get;}
    }
}