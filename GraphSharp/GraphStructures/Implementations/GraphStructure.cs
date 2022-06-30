using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Nodes;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Create nodes for graph structure. Entry for any other logic of graph structure.
    /// </summary>
    public partial class GraphStructure<TNode,TEdge> : GraphStructureBase<TNode,TEdge>, GraphSharp.Common.ICloneable<GraphStructure<TNode,TEdge>>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        public GraphStructure(IGraphConfiguration<TNode,TEdge> configuration) : base(configuration)
        {}
        
        /// <summary>
        /// Copy constructor. Will make shallow copy of graphStructure
        /// </summary>
        public GraphStructure(GraphStructureBase<TNode, TEdge> graphStructure) : base(graphStructure) 
        {}

        public GraphStructure<TNode,TEdge> SetSources(INodeSource<TNode> nodes, IEdgeSource<TNode,TEdge> edges){
            Nodes = nodes;
            Edges = edges;
            return this;
        }
        
        /// <summary>
        /// Create some count of nodes. This method will replace current <see cref="IGraphStructure{,}.Nodes"/> and <see cref="IGraphStructure{,}.Edges"/> with new ones.
        /// </summary>
        /// <param name="count">Count of nodes to create</param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> Create(int nodesCount)
        {
            Nodes = Configuration.CreateNodeSource();
            Edges = Configuration.CreateEdgeSource();
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
        public GraphStructureOperation<TNode,TEdge> Do => new GraphStructureOperation<TNode, TEdge>(this);
        
        /// <summary>
        /// Get converters for current graph structure. This class allows you to convert current graph structure to different representations or rebuild current one from other representations as well.
        /// </summary>
        public GraphStructureConverters<TNode,TEdge> Converter=> new(this);
        
        /// <summary>
        /// Create new induced subgraph from this graph structure.
        /// </summary>
        /// <param name="toInduce">Select nodes to induce</param>
        /// <returns>A new induced graph that is subgraph of current graph. Perform cloning operations so result is independent from original graph.</returns>
        public GraphStructure<TNode,TEdge> Induce(Predicate<TNode> toInduce){
            var result = new GraphStructure<TNode,TEdge>(Configuration);
            var nodes = Nodes
                .Where(x=>toInduce(x))
                .Select(x=>Configuration.CloneNode(x));
            
            foreach(var n in nodes){
                result.Nodes.Add(n);
            }
            
            var edges = Edges
                .Where(x=>toInduce(x.Source) && toInduce(x.Target))
                .Select(x=>Configuration.CloneEdge(x,result.Nodes));

            foreach(var e in edges){
                result.Edges.Add(e);
            }
            return result;
        }
        
        /// <summary>
        /// Clones graph structure
        /// </summary>
        /// <returns>Copy of current graph structure</returns>
        public GraphStructure<TNode,TEdge> Clone()
        {
            var result = new GraphStructure<TNode,TEdge>(Configuration);
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
        /// Replace current Nodes and Edges with new ones. Does not clear old Nodes and Edges.
        /// </summary>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> Clear(){
            Nodes = Configuration.CreateNodeSource();
            Edges = Configuration.CreateEdgeSource();
            return this;
        }

    }
}