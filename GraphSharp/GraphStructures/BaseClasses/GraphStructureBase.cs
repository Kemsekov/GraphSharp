using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Base class for graph structure.
    /// </summary>
    public abstract class GraphStructureBase<TNode, TEdge> : IGraphStructure<TNode,TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {

        /// <summary>
        /// Configuration for all needed operations with nodes and edges
        /// </summary>
        public IGraphConfiguration<TNode,TEdge> Configuration{get;protected set;}
        public INodeSource<TNode> Nodes { get; protected set; }
        public IEdgeSource<TEdge> Edges { get; protected set; }
        /// <summary>
        /// Base copy constructor. Will make shallow copy of structureBase
        /// </summary>
        public GraphStructureBase(GraphStructureBase<TNode, TEdge> structureBase)
        {
            Nodes         = structureBase.Nodes;
            Edges         = structureBase.Edges;
            Configuration = structureBase.Configuration;
        }

        public GraphStructureBase(IGraphConfiguration<TNode,TEdge> configuration)
        {
            Configuration = configuration;
            Nodes = configuration.CreateNodeSource();
            Edges = configuration.CreateEdgeSource();
        }
        /// <summary>
        /// Calculate parents count (degree) for each node
        /// </summary>
        /// <returns><see cref="IDictionary{,}"/> where TKey is node id and TValue is parents count</returns>
        public IDictionary<int,int> CountParents(){
            ConcurrentDictionary<int,int> c = new();
            foreach(var n in Nodes)
                c[n.Id]=0;
            
            foreach(var e in Edges){
                c[e.Child.Id]++;
            }
            
            return c;
        }
        public float MeanNodeEdgesCount()
            => (float)(Edges.Count) / (Nodes.Count==0 ? 1 : Nodes.Count);  
        /// <summary>
        /// Checks for data integrity in Nodes and Edges. If there is a case when some edge is references to unknown node throws an exception.
        /// </summary>
        public void CheckForIntegrity()
        {
            foreach(var e in Edges){
                if (!Nodes.TryGetNode(e.Parent.Id,out var _)){
                    throw new System.InvalidOperationException($"{e.Parent.Id} found among Edges but not found among Nodes");
                }
                if (!Nodes.TryGetNode(e.Child.Id,out var _)){
                    throw new System.InvalidOperationException($"{e.Child.Id} found among Edges but not found among Nodes");
                }
            }
        }      
    }
}