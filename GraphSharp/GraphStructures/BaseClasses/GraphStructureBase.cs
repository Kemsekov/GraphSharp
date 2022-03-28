using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
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
        /// <see cref="Random"/> that used to implement's any logic when it reqires random values
        /// </summary>
        /// <value></value>
        public Random Rand { get;init; }
        /// <summary>
        /// Method that used to create instance of <see cref="TNode"/> from it's <see cref="INode.Id"/> as argument
        /// </summary>
        /// <value></value>
        public Func<int, TNode> CreateNode { get;init; }
        /// <summary>
        /// Method that used to create new <see cref="TEdge"/> from two <see cref="TNode"/>, where first node is parent and second is it's neighbor
        /// (parent,node)=>new Edge...
        /// </summary>
        public Func<TNode, TNode, TEdge> CreateEdge { get;init; }
        /// <summary>
        /// Method that used to get weight from particular <see cref="TEdge"/>
        /// </summary>
        /// <value></value>
        public Func<TEdge, float> GetWeight { get; init;}
        /// <summary>
        /// Method that used to determite how to calculate distance between two <see cref="TNode"/>
        /// </summary>
        /// <value></value>
        public Func<TNode, TNode, float> Distance { get;init; }

        public IEnumerable<TNode> WorkingGroup { get; protected set; }

        public IList<TNode> Nodes { get; protected set; }

        /// <summary>
        /// Base copy constructor. Will make shallow copy of structureBase
        /// </summary>
        /// <param name="structureBase"></param>
        public GraphStructureBase(GraphStructureBase<TNode, TEdge> structureBase)
        {
            Rand         = structureBase.Rand;
            CreateNode   = structureBase.CreateNode;
            CreateEdge   = structureBase.CreateEdge;
            WorkingGroup = structureBase.WorkingGroup;
            Nodes        = structureBase.Nodes;
            GetWeight    = structureBase.GetWeight;
            Distance     = structureBase.Distance;
        }

        public GraphStructureBase(Func<int, TNode> createNode, Func<TNode, TNode, TEdge> createEdge, Func<TEdge, float> getWeight, Func<TNode, TNode, float> distance, Random rand = null)
        {
            Rand       = rand ?? new Random();
            CreateNode = createNode;
            CreateEdge = createEdge;
            Distance   = distance;
            GetWeight  = getWeight;
        }
        /// <summary>
        /// Convert each edge's parent and node values to tuple (int parent, int node)
        /// </summary>
        /// <returns>A list of tuples where first element is a parent of edge and second is node to other edge</returns>
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
                    adjacencyMatrix[i, e.Node.Id] = GetWeight(e);
                }
            }
            return adjacencyMatrix;
        }
        /// <summary>
        /// Builds connections dictionary from graph structure. Result of this method only make sense if graph is a tree, because in this representation any node can have only one parent.
        /// </summary>
        /// <returns><see cref="IDictionary{,}"/> where TKey is node id and TValue is parent id</returns>
        public IDictionary<int,int> ToTreeConnectionsDictionary(){
            Dictionary<int,int> c = new();
            foreach(var n in Nodes){
                foreach(var e in n.Edges){
                    c[e.Node.Id]=e.Parent.Id;
                }
            }
            return c;
        }
        /// <summary>
        /// Calculate parents count (degree) for each node
        /// </summary>
        /// <returns><see cref="IDictionary{,}"/> where TKey is node id and TValue is parents count</returns>
        public IDictionary<int,int> CountDegrees(){
            ConcurrentDictionary<int,int> c = new();
            foreach(var n in Nodes)
                c[n.Id]=0;
            
            Parallel.ForEach(Nodes,node=>{
                foreach(var e in node.Edges){
                    c[e.Node.Id]++;
                }
            });
            return c;
        }
        /// <returns>Whatever given graph structure is a tree</returns>
        public bool IsTree(){
            return CountDegrees().All(x=>x.Value==1);
        }
        public int TotalEdgesCount()
        {
            var result = 0;
            foreach (var n in Nodes)
            {
                result += n.Edges.Count();
            }
            return result;
        }
        public float MeanEdgesCountPerNode()
            => (float)(TotalEdgesCount()) / Nodes.Count;        
    }
}