using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;

using GraphSharp.Graphs;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Create nodes for graph structure. Entry for any other logic of graph structure.
    /// </summary>
    public class Graph<TNode,TEdge> : IGraph<TNode,TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        /// <summary>
        /// Configuration for this graph.
        /// </summary>
        public IGraphConfiguration<TNode,TEdge> Configuration{get;protected set;}
        public INodeSource<TNode> Nodes { get; protected set; }
        public IEdgeSource<TEdge> Edges { get; protected set; }
        /// <summary>
        /// Create new graph with specified nodes and edges creation functions
        /// </summary>
        public Graph(Func<int,TNode> createNode, Func<TNode,TNode,TEdge> createEdge)
        : this(new GraphConfiguration<TNode,TEdge>(new Random(),createEdge,createNode))
        {
        }
        /// <summary>
        /// Just init new graph with empty Nodes and Edges using given configuration.
        /// </summary>
        /// <param name="configuration"></param>
        public Graph(IGraphConfiguration<TNode,TEdge> configuration)
        {
            Configuration = configuration;
            Nodes = configuration.CreateNodeSource();
            Edges = configuration.CreateEdgeSource();
        }
        
        /// <summary>
        /// Copy constructor. Will make shallow copy of Graph
        /// </summary>
        public Graph(IGraph<TNode, TEdge> Graph)
        {
            Nodes         = Graph.Nodes;
            Edges         = Graph.Edges;
            Configuration = Graph.Configuration;
        }
        /// <summary>
        /// Set current graph's Nodes and Edges
        /// </summary>
        public Graph<TNode,TEdge> SetSources(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges){
            Nodes = nodes;
            Edges = edges;
            return this;
        }

        /// <summary>
        /// Returns operations class for this graph structure. This class contains methods to perform different algorithms on current graph structure.
        /// </summary>
        public GraphOperation<TNode,TEdge> Do => new GraphOperation<TNode, TEdge>(this);
        
        /// <summary>
        /// Get converters for current graph structure. This class allows you to convert current graph structure to different representations or rebuild current one from other representations as well.
        /// </summary>
        public GraphConverters<TNode,TEdge> Converter=> new(this);

    }
}