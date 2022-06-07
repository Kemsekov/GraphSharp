using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Edges;
using GraphSharp.Exceptions;
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
        /// Calculate sources count (degree) for each node
        /// </summary>
        /// <returns><see cref="IDictionary{,}"/> where TKey is node id and TValue is sources count</returns>
        public IDictionary<int,int> Countsources(){
            ConcurrentDictionary<int,int> c = new();
            foreach(var n in Nodes)
                c[n.Id]=0;
            
            foreach(var e in Edges){
                c[e.Target.Id]++;
            }
            
            return c;
        }
        public float MeanNodeEdgesCount()
            => (float)(Edges.Count) / (Nodes.Count==0 ? 1 : Nodes.Count);  
        
        /// <summary>
        /// Checks for data integrity in Nodes and Edges. If there is a case when some edge is references to unknown node throws an exception. If there is duplicate node throws an exception. If there is duplicate edge throws an exception.
        /// </summary>
        public void CheckForIntegrity()
        {
            var actual = Nodes.Select(x=>x.Id);
            var expected = actual.Distinct();
            if(actual.Count()!=expected.Count())
                throw new GraphDataIntegrityException("Nodes contains duplicates");

            var actualEdges = Edges.Select(x=>(x.Source.Id,x.Target.Id));
            var expectedEdges = actualEdges.Distinct();
            if(actualEdges.Count()!=expectedEdges.Count())
                throw new GraphDataIntegrityException("Edges contains duplicates");

            foreach(var e in Edges){
                if (!Nodes.TryGetNode(e.Source.Id,out var _)){
                    throw new GraphDataIntegrityException($"{e.Source.Id} found among Edges but not found among Nodes");
                }
                if (!Nodes.TryGetNode(e.Target.Id,out var _)){
                    throw new GraphDataIntegrityException($"{e.Target.Id} found among Edges but not found among Nodes");
                }
            }
        }
        /// <summary>
        /// Checks if graph colored in a right way. Throws an exception if there is a case when some node is not colored in a right way.
        /// </summary>
        public void EnsureRightColoring()
        {
            foreach(var n in Nodes){
                var color = Configuration.GetNodeColor(n);
                var edges = Edges[n.Id];
                if(edges.Any(x=>Configuration.GetNodeColor(x.Target)==color)){
                    throw new WrongGraphColoringException($"Wrong graph coloring! Node {n.Id} with color {color} have edge with the same color!");               
                }
            }
        }      
    }
}