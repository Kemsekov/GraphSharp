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
        /// Calculate parents count (degree) for each node
        /// </summary>
        /// <returns><see cref="IDictionary{,}"/> where TKey is node id and TValue is parents count</returns>
        public IDictionary<int,int> CountParents(){
            ConcurrentDictionary<int,int> c = new();
            foreach(var n in Nodes)
                c[n.Id]=0;
            
            Parallel.ForEach(Nodes,node=>{
                foreach(var e in node.Edges){
                    lock(e.Child)
                        c[e.Child.Id]++;
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