using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
namespace GraphSharp.Graphs
{
    /// <summary>
    /// Create nodes for graph structure. Entry for any other logic of graph structure.
    /// </summary>
    public class Graph<TNode,TEdge> : IGraph<TNode,TEdge>, GraphSharp.Common.ICloneable<Graph<TNode,TEdge>>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        /// <summary>
        /// Configuration for this graph.
        /// </summary>
        public IGraphConfiguration<TNode,TEdge> Configuration{get;protected set;}
        public INodeSource<TNode> Nodes { get; protected set; }
        public IEdgeSource<TNode,TEdge> Edges { get; protected set; }
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
        public Graph<TNode,TEdge> SetSources(INodeSource<TNode> nodes, IEdgeSource<TNode,TEdge> edges){
            Nodes = nodes;
            Edges = edges;
            return this;
        }
        
        /// <summary>
        /// Clears graph and creates some count of nodes.
        /// </summary>
        /// <param name="count">Count of nodes to create</param>
        public Graph<TNode,TEdge> Create(int nodesCount)
        {
            Clear();
            //create nodes
            for (int i = 0; i < nodesCount; i++)
            {
                var node = Configuration.CreateNode(i);
                Nodes.Add(node);
            }
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
        
        /// <summary>
        /// Get induced subgraph from this graph structure.
        /// </summary>
        /// <param name="nodes">Nodes to induce</param>
        /// <returns>Induced subgraph of current graph</returns>
        public Graph<TNode,TEdge> Induce(params int[] nodes){
            var result = new Graph<TNode,TEdge>(Configuration);
            var toInduce = new byte[Nodes.MaxNodeId+1];
            foreach(var n in nodes){
                toInduce[n] = 1;
                result.Nodes.Add(Nodes[n]);
            }
            
            var edges = Edges.Where(x=>toInduce[x.Source.Id]==1 && toInduce[x.Target.Id]==1);

            foreach(var e in edges){
                result.Edges.Add(e);
            }
            return result;
        }

        /// <summary>
        /// Clones graph structure
        /// </summary>
        /// <returns>Copy of current graph structure</returns>
        public Graph<TNode,TEdge> Clone()
        {
            var result = new Graph<TNode,TEdge>(Configuration);
            var nodes = Nodes
                .Select(x=>Configuration.CloneNode(x));
            
            foreach(var n in nodes){
                result.Nodes.Add(n);
            }
            
            var edges = Edges.Select(x=>Configuration.CloneEdge(x,result.Nodes));

            foreach(var e in edges){
                result.Edges.Add(e);
            }
            return result;
        }
        
        /// <summary>
        /// Clears current Nodes and Edges with new ones. Does not clear old Nodes and Edges.
        /// </summary>
        public Graph<TNode,TEdge> Clear(){
            Nodes.Clear();
            Edges.Clear();
            return this;
        }

    }
}