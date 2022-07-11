using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Exceptions;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public static class GraphStructureExtensions
    {

        /// <returns>True if graph is directed, else false</returns>
        public static bool IsDirected<TNode,TEdge>(this IGraphStructure<TNode,TEdge> graph)
        where TNode : INode
        where TEdge : IEdge<TNode>
        {
            foreach(var e in graph.Edges){
                if(graph.Edges.TryGetEdge(e.Target.Id,e.Source.Id,out _)){
                    return false;
                }
            }
            return true;
        }
        /// <returns>True if graph is undirected, else false</returns>
        public static bool IsUndirected<TNode,TEdge>(this IGraphStructure<TNode,TEdge> graph)
        where TNode : INode
        where TEdge : IEdge<TNode>
        {
            foreach(var e in graph.Edges){
                if(!graph.Edges.TryGetEdge(e.Target.Id,e.Source.Id,out _))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Checks for data integrity in Nodes and Edges. If there is a case when some edge is references to unknown node throws an exception. If there is duplicate node throws an exception. If there is duplicate edge throws an exception.
        /// </summary>
        public static void CheckForIntegrity<TNode,TEdge>(this IGraphStructure<TNode,TEdge> graph)
        where TNode : INode
        where TEdge : IEdge<TNode>
        {
            var actual = graph.Nodes.Select(x=>x.Id);
            var expected = actual.Distinct();
            if(actual.Count()!=expected.Count())
                throw new GraphDataIntegrityException("Nodes contains duplicates");

            foreach(var n in graph.Nodes){
                var edges = graph.Edges[n.Id];
                var actualEdges = edges.Select(x=>(x.Source.Id,x.Target.Id));
                var expectedEdges = actualEdges.Distinct();
                if(actualEdges.Count()!=expectedEdges.Count()){
                    StringBuilder b = new();
                    foreach(var a in actualEdges)
                        b.Append(a.ToString()+'\n');
                    b.Append("---------\n");
                    foreach(var e in expectedEdges)
                        b.Append(e.ToString()+'\n');
                    throw new GraphDataIntegrityException($"Edges contains duplicates : {actualEdges.Count()} != {expectedEdges.Count()} \n{b.ToString()}");
                }
            }
            foreach(var e in graph.Edges){
                if (!graph.Nodes.TryGetNode(e.Source.Id,out var _)){
                    throw new GraphDataIntegrityException($"{e.Source.Id} found among Edges but not found among Nodes");
                }
                if (!graph.Nodes.TryGetNode(e.Target.Id,out var _)){
                    throw new GraphDataIntegrityException($"{e.Target.Id} found among Edges but not found among Nodes");
                }
            }
        }
        public static void ValidatePath<TNode,TEdge>(this IGraphStructure<TNode,TEdge> graph,IList<TNode> path)
        where TNode : INode
        where TEdge : IEdge<TNode>
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];
                if(!graph.Edges.TryGetEdge(current.Id,next.Id,out var edge)){
                    throw new ArgumentException($"Edge {current.Id}->{next.Id} not found! Path is not valid!");
                }
            }
        }
        /// <summary>
        /// Checks if graph colored in a right way. Throws an exception if there is a case when some node is not colored in a right way.
        /// </summary>
        public static void EnsureRightColoring<TNode,TEdge>(this IGraphStructure<TNode,TEdge> graph)
        where TNode : INode
        where TEdge : IEdge<TNode>
        {
            foreach(var n in graph.Nodes){
                var color = n.Color;
                var edges = graph.Edges[n.Id];
                if(edges.Any(x=>x.Target.Color==color)){
                    throw new WrongGraphColoringException($"Wrong graph coloring! Node {n.Id} with color {color} have edge with the same color!");               
                }
            }
        }  
        public static float MeanNodeEdgesCount<TNode,TEdge>(this IGraphStructure<TNode,TEdge> graph)
        where TNode : INode
        where TEdge : IEdge<TNode>
            => (float)(graph.Edges.Count) / (graph.Nodes.Count==0 ? 1 : graph.Nodes.Count);  
        
        public static int[] GetNodesIdWhere<TNode,TEdge>(this IGraphStructure<TNode,TEdge> graph,Predicate<TNode> predicate)
        where TNode : INode
        where TEdge : IEdge<TNode>
        {
            return graph.Nodes.Where(x=>predicate(x)).Select(x=>x.Id).ToArray();
        }
    }
}