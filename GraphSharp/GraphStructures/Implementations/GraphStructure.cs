using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures.Interfaces;
using GraphSharp.Nodes;
using MathNet.Numerics.LinearAlgebra.Single;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Create nodes for graph structure / set working group
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
        /// Will set <see cref="IGraphStructure{}.WorkingGroup"/> to <see cref="IGraphStructure{}.Nodes"/>
        /// </summary>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForEach()
        {
            WorkingGroup = Nodes;
            return new(this);
        }

        /// <summary>
        /// Will set <see cref="IGraphStructure{}.WorkingGroup"/> to some particular node from <see cref="IGraphStructure{}.Nodes"/> with id == nodeId
        /// </summary>
        /// <param name="nodeId">Node id</param>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForOne(int nodeId)
        {
            WorkingGroup = Nodes.Where(x => x.Id == nodeId);
            return new(this);
        }

        /// <summary>
        /// Will set <see cref="IGraphStructure{}.WorkingGroup"/> to some subset of <see cref="IGraphStructure{}.Nodes"/>
        /// </summary>
        /// <param name="selector">Method that used to select subset of nodes from current <see cref="IGraphStructure{}.Nodes"/></param>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForNodes(Func<IEnumerable<TNode>, IEnumerable<TNode>> selector)
        {
            WorkingGroup = selector(Nodes);
            return new(this);
        }
        
        public GraphStructureConverters<TNode,TEdge> Converter=> new(this);
        /// <summary>
        /// Clears current <see cref="IGraphStructure{}.WorkingGroup"/> 
        /// </summary>
        /// <returns><see cref="GraphStructure{,}"/> that can be used to reset <see cref="IGraphStructure{}.WorkingGroup"/> </returns>
        public GraphStructure<TNode,TEdge> ClearWorkingGroup(){
            WorkingGroup = Enumerable.Empty<TNode>();
            return new(this);
        }
    }
}