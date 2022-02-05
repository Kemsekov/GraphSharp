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
//rename it to GraphStructure. Make subclasses for it that divide logic.
//don't store everything in one class.

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Create graph structure
    /// </summary>
    public partial class GraphStructure : GraphStructureBase
    {
        public GraphStructure(Func<int, INode> createNode = null, Func<INode, INode, IEdge> createEdge = null, Func<IEdge, float> getWeight = null, Func<INode, INode, float> distance = null, Random rand = null) : base(createNode, createEdge, getWeight, distance, rand)
        {
        }

        /// <summary>
        /// Replace current <see cref="GraphStructure.Nodes"/> to nodes
        /// </summary>
        /// <param name="nodes">What need to be used as Nodes</param>
        /// <returns></returns>
        public GraphStructure UseNodes(IList<INode> nodes)
        {
            Nodes = nodes;
            ForEach();
            return this;
        }
        /// <summary>
        /// Create count nodes. This method will replace existing Nodes in current instance of GraphStructure.
        /// </summary>
        /// <param name="count">Count of codes to create</param>
        /// <returns></returns>
        public GraphStructure CreateNodes(int count)
        {
            var nodes = new List<INode>(count);

            //create nodes
            for (int i = 0; i < count; i++)
            {
                var node = CreateNode(i);
                nodes.Add(node);
            }
            return UseNodes(nodes);
        }

        /// <summary>
        /// Will set <see cref="GraphStructure.WorkingGroup"/> to <see cref="GraphStructure.Nodes"/>
        /// </summary>
        /// <returns></returns>
        public GraphStructureOperation ForEach()
        {
            WorkingGroup = Nodes;
            return new(this);
        }

        /// <summary>
        /// Will set <see cref="GraphStructure.WorkingGroup"/> to particular node from <see cref="GraphStructure.Nodes"/> with id == nodeId
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public GraphStructureOperation ForOne(int nodeId)
        {
            WorkingGroup = Nodes.Where(x => x.Id == nodeId);
            return new(this);
        }

        /// <summary>
        /// Will set <see cref="GraphStructure.WorkingGroup"/> to some subset of <see cref="GraphStructure.Nodes"/>
        /// </summary>
        /// <param name="selector">receive <see cref="GraphStructure.Nodes"/> and returns some set of values from them</param>
        /// <returns></returns>
        public GraphStructureOperation ForNodes(Func<IList<INode>, IEnumerable<INode>> selector)
        {
            WorkingGroup = selector(Nodes);
            return new(this);
        }
        
        /// <summary>
        /// Create nodes and edges from adjacency matrix and set them to <see cref="GraphStructureFactory.Nodes"/>
        /// </summary>
        /// <param name="adjacencyMatrix"></param>
        /// <returns></returns>
        public GraphStructureOperation FromAdjacencyMatrix(Matrix adjacencyMatrix,Action<IEdge,float> applyWeight = null){
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
            return new(this);
        }
       


    }
}