using System;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Edges;
using GraphSharp.Nodes;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Create nodes for graph structure. Entry for any other logic of graph structure.
    /// </summary>
    public partial class GraphStructure<TNode,TEdge> : GraphStructureBase<TNode,TEdge> 
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public GraphStructure(IGraphConfiguration<TNode,TEdge> configuration) : base(configuration)
        {}
        public GraphStructure(GraphStructureBase<TNode, TEdge> graphStructure) : base(graphStructure) 
        {}

        /// <summary>
        /// Replace current <see cref="IGraphStructure{}.Nodes"/> to nodes
        /// </summary>
        /// <param name="nodes">What need to be used as <see cref="IGraphStructure{}.Nodes"/></param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> UseNodes(IList<TNode> nodes)
        {
            Nodes = nodes;
            return this;
        }
        /// <summary>
        /// Create some count of nodes. This method will replace current <see cref="IGraphStructure{}.Nodes"/>.
        /// </summary>
        /// <param name="count">Count of codes to create</param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> CreateNodes(int count)
        {
            var nodes = new List<TNode>(count);

            //create nodes
            for (int i = 0; i < count; i++)
            {
                var node = Configuration.CreateNode(i);
                nodes.Add(node);
            }
            return UseNodes(nodes);
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
        /// Reindex nodes in graph structure.
        /// </summary>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> ReindexNodes(){
            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Id = i;
            return this;
        }
    }
}