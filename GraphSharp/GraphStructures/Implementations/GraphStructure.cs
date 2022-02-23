using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Create nodes for graph structure / set working group
    /// </summary>
    public partial class GraphStructure<TNode,TEdge> : GraphStructureBase<TNode,TEdge> 
    where TNode : INode<TEdge>
    where TEdge : IEdge<TNode>
    {
        public GraphStructure(Func<int, TNode> createNode, Func<TNode, TNode, TEdge> createEdge, Func<TEdge, float> getWeight = null, Func<TNode, TNode, float> distance = null, Random rand = null) : base(createNode, createEdge, getWeight, distance, rand)
        {}
        public GraphStructure(GraphStructureBase<TNode, TEdge> graphStructure) : base(graphStructure) 
        {}

        /// <summary>
        /// Replace current <see cref="IGraphStructure.Nodes"/> to nodes
        /// </summary>
        /// <param name="nodes">What need to be used as <see cref="IGraphStructure.Nodes"/></param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> UseNodes(IList<TNode> nodes)
        {
            Nodes = nodes;
            return this;
        }
        /// <summary>
        /// Create some count of nodes. This method will replace current <see cref="IGraphStructure.Nodes"/>.
        /// </summary>
        /// <param name="count">Count of codes to create</param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> CreateNodes(int count)
        {
            var nodes = new List<TNode>(count);

            //create nodes
            for (int i = 0; i < count; i++)
            {
                var node = CreateNode(i);
                nodes.Add(node);
            }
            return UseNodes(nodes);
        }

        /// <summary>
        /// Will set <see cref="IGraphStructure.WorkingGroup"/> to <see cref="IGraphStructure.Nodes"/>
        /// </summary>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForEach()
        {
            WorkingGroup = Nodes;
            return new(this);
        }

        /// <summary>
        /// Will set <see cref="IGraphStructure.WorkingGroup"/> to some particular node from <see cref="IGraphStructure.Nodes"/> with id == nodeId
        /// </summary>
        /// <param name="nodeId">Node id</param>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForOne(int nodeId)
        {
            WorkingGroup = Nodes.Where(x => x.Id == nodeId);
            return new(this);
        }

        /// <summary>
        /// Will set <see cref="IGraphStructure.WorkingGroup"/> to some subset of <see cref="IGraphStructure.Nodes"/>
        /// </summary>
        /// <param name="selector">Method that used to select subset of nodes from current <see cref="IGraphStructure.Nodes"/></param>
        /// <returns></returns>
        public GraphStructureOperation<TNode,TEdge> ForNodes(Func<IEnumerable<TNode>, IEnumerable<TNode>> selector)
        {
            WorkingGroup = selector(Nodes);
            return new(this);
        }
        
        /// <summary>
        /// Create nodes and edges from adjacency matrix and set them to <see cref="GraphStructureFactory.Nodes"/>
        /// </summary>
        /// <param name="adjacencyMatrix"></param>
        /// <returns></returns>
        public GraphStructure<TNode,TEdge> FromAdjacencyMatrix(Matrix adjacencyMatrix,Action<TEdge,float> applyWeight = null){
            if(adjacencyMatrix.RowCount!=adjacencyMatrix.ColumnCount)
                throw new ArgumentException("adjacencyMatrix argument must be square matrix!",nameof(adjacencyMatrix));
            
            applyWeight ??= (edge,weight)=>{};
            int width = adjacencyMatrix.RowCount;
            CreateNodes(width);

            for(int i = 0;i<Nodes.Count;i++){
                for(int b = 0;b<width;b++){
                    if(adjacencyMatrix[i,b]!=0){
                        var edge = CreateEdge(Nodes[b],Nodes[i]);
                        Nodes[i].Edges.Add(edge);
                        applyWeight(edge,adjacencyMatrix[i,b]);
                    }
                }
            }
            return this;
        }
        


    }
}