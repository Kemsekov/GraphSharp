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
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public GraphStructure(IGraphConfiguration<TNode,TEdge> configuration) : base(configuration)
        {}
        /// <summary>
        /// Copy constructor. Will make shallow copy of graphStructure
        /// </summary>
        public GraphStructure(GraphStructureBase<TNode, TEdge> graphStructure) : base(graphStructure) 
        {}

        public GraphStructure<TNode,TEdge> SetSources(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges){
            Nodes = nodes;
            Edges = edges;
            return this;
        }
        /// <summary>
        /// Create some count of nodes. This method will replace current <see cref="IGraphStructure{,}.Nodes"/>.
        /// </summary>
        /// <param name="count">Count of codes to create</param>
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
        /// Returns operations class for nodes
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> Do => new GraphStructureOperation<TNode, TEdge>(this);
        /// <summary>
        /// Get converter for current graph structure
        /// </summary>
        public GraphStructureConverters<TNode,TEdge> Converter=> new(this);



        /// <summary>
        /// Clones graph structure
        /// </summary>
        /// <returns></returns>
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

        public GraphStructure<TNode,TEdge> Clear(){
            Nodes = Configuration.CreateNodeSource();
            Edges = Configuration.CreateEdgeSource();
            return this;
        }

    }
}