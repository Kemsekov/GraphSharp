using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures.Interfaces;
using GraphSharp.Nodes;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Base class for graph structure.
    /// </summary>
    public abstract class GraphStructureBase<TNode, TEdge> : IGraphStructure<TNode>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {

        /// <summary>
        /// Configuration for all needed operations with nodes and edges
        /// </summary>
        public IGraphConfiguration<TNode,TEdge> Configuration{get;protected set;}
        /// <summary>
        /// Subset of <see cref="IGraphStructure{,}.Nodes"/> that used to modify nodes.
        /// </summary>
        public IEnumerable<TNode> WorkingGroup { get; protected set; }
        public IList<TNode> Nodes { get; protected set; }
        /// <summary>
        /// Base copy constructor. Will make shallow copy of structureBase
        /// </summary>
        public GraphStructureBase(GraphStructureBase<TNode, TEdge> structureBase)
        {
            WorkingGroup  = structureBase.WorkingGroup;
            Nodes         = structureBase.Nodes;
            Configuration = structureBase.Configuration;
        }

        public GraphStructureBase(IGraphConfiguration<TNode,TEdge> configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// Convert each edge's parent and node values to tuple (int parent, int node)
        /// </summary>
        /// <returns>A list of tuples where first element is a parent of edge and second is node of edge</returns>
        public IList<(int parent,int node)> ToConnectionsList(){
            var result = new List<(int parent,int node)>();
            foreach(var n in Nodes){
                foreach(var e in n.Edges){
                    result.Add((e.Parent.Id,e.Node.Id));
                }
            }
            return result;
        }
         /// <summary>
        /// Converts current <see cref="IGraphStructure.Nodes"/> to adjacency matrix using <see cref="IGraphStructure.GetWeight"/> to determine matrix value per <see cref="IEdge"/>
        /// </summary>
        public Matrix ToAdjacencyMatrix()
        {
            Matrix adjacencyMatrix;

            //if matrix size will take more than 64 mb of RAM then make it sparse
            if (Nodes.Count > 4096)
                adjacencyMatrix = SparseMatrix.Create(Nodes.Count, Nodes.Count, 0);
            else
                adjacencyMatrix = DenseMatrix.Create(Nodes.Count, Nodes.Count, 0);

            for (int i = 0; i < Nodes.Count; i++)
            {
                foreach (var e in Nodes[i].Edges)
                {
                    adjacencyMatrix[i, e.Node.Id] = Configuration.GetEdgeWeight(e);
                }
            }
            return adjacencyMatrix;
        }
        /// <summary>
        /// Calculate parents count (degree) for each node
        /// </summary>
        /// <returns><see cref="IDictionary{,}"/> where TKey is node id and TValue is parents count</returns>
        public IDictionary<int,int> CountParents(){
            ConcurrentDictionary<int,int> c = new();
            foreach(var n in Nodes)
                c[n.Id]=0;
            
            Parallel.ForEach(Nodes,node=>{
                foreach(var e in node.Edges){
                    lock(e.Node)
                        c[e.Node.Id]++;
                }
            });
            return c;
        }
        /// <returns>Total edges count</returns>
        public int EdgesCount()
        {
            var result = 0;
            foreach (var n in Nodes)
            {
                result += n.Edges.Count();
            }
            return result;
        }
        public float MeanNodeEdgesCount()
            => (float)(EdgesCount()) / (Nodes.Count==0 ? 1 : Nodes.Count);        
    }
}