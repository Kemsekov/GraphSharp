using System.Collections.Generic;


namespace GraphSharp.Graphs
{
    /// <summary>
    /// Graph structure holder interface.
    /// </summary>
    public interface IGraph<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        INodeSource<TNode> Nodes { get; }
        IEdgeSource<TEdge> Edges { get; }
        IGraphConfiguration<TNode,TEdge> Configuration{get;}
        GraphOperation<TNode,TEdge> Do{get;}
        GraphConverters<TNode,TEdge> Converter{get;}
        /// <summary>
        /// Set current graph's Nodes and Edges
        /// </summary>
        public Graph<TNode,TEdge> SetSources(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges);
    }
}