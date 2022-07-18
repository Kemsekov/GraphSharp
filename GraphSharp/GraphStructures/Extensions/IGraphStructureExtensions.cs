using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Exceptions;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs;
public static class GraphExtensions
{
    /// <returns>True if graph is directed, else false</returns>
    public static bool IsDirected<TNode, TEdge>(this IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        foreach (var e in graph.Edges)
        {
            if (graph.Edges.TryGetEdge(e.Target.Id, e.Source.Id, out _))
            {
                return false;
            }
        }
        return true;
    }
    /// <returns>True if graph is undirected, else false</returns>
    public static bool IsUndirected<TNode, TEdge>(this IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        foreach (var e in graph.Edges)
        {
            if (!graph.Edges.TryGetEdge(e.Target.Id, e.Source.Id, out _))
                return false;
        }
        return true;
    }
    /// <summary>
    /// Checks for data integrity in Nodes and Edges. If there is a case when some edge is references to unknown node throws an exception. If there is duplicate node throws an exception. If there is duplicate edge throws an exception. If there is unknown reference between nodes that does not present in the edges list throws an exception;
    /// </summary>
    public static void CheckForIntegrity<TNode, TEdge>(this IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        var actual = graph.Nodes.Select(x => x.Id);
        var expected = actual.Distinct();
        if (actual.Count() != expected.Count())
            throw new GraphDataIntegrityException("Nodes contains duplicates");

        foreach (var n in graph.Nodes)
        {
            var edges = graph.Edges[n.Id];
            var actualEdges = edges.Select(x => (x.Source.Id, x.Target.Id));
            var expectedEdges = actualEdges.Distinct();
            if (actualEdges.Count() != expectedEdges.Count())
            {
                StringBuilder b = new();
                foreach (var a in actualEdges)
                    b.Append(a.ToString() + '\n');
                b.Append("---------\n");
                throw new GraphDataIntegrityException($"Edges contains duplicates : {actualEdges.Count()} != {expectedEdges.Count()} \n{b.ToString()}");
            }
        }
        foreach (var e in graph.Edges)
        {
            if (!graph.Nodes.TryGetNode(e.Source.Id, out var _))
            {
                throw new GraphDataIntegrityException($"{e.Source.Id} found among Edges but not found among Nodes");
            }
            if (!graph.Nodes.TryGetNode(e.Target.Id, out var _))
            {
                throw new GraphDataIntegrityException($"{e.Target.Id} found among Edges but not found among Nodes");
            }
        }
        foreach (var n in graph.Nodes)
        {
            foreach (var source in graph.Edges.GetSourcesId(n.Id))
            {
                if (!graph.Edges.TryGetEdge(source, n.Id, out var _))
                    throw new GraphDataIntegrityException($"Edge {source}->{n.Id} is present in sources list but not found among edges");
            }
        }
    }
    /// <summary>
    /// Validates that given path is a valid path for current graph.
    /// </summary>
    public static void ValidatePath<TNode, TEdge>(this IGraph<TNode, TEdge> graph, IList<TNode> path)
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            var current = path[i];
            var next = path[i + 1];
            if (!graph.Edges.TryGetEdge(current.Id, next.Id, out var edge))
            {
                throw new ArgumentException($"Edge {current.Id}->{next.Id} not found! Path is not valid!");
            }
        }
    }
    /// <summary>
    /// Validates that given path is a valid cycle for given graph.
    /// </summary>
    public static void ValidateCycle<TNode, TEdge>(this IGraph<TNode, TEdge> graph, IList<TNode> cycle)
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        var head = cycle.First();
        try
        {
            try
            {
                graph.ValidatePath(cycle);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Given cycle is not a correct path!\n" + e.Message);
            }
            if (cycle.Last().Id != head.Id)
            {
                throw new ArgumentException("Cycle must start and end with same node");
            }
            if (cycle.Count < 3)
            {
                throw new ArgumentException("Cycle length must be at least 3");
            }
            var headless = cycle.Except(new[] { head });
            if (headless.Count() != headless.DistinctBy(x => x.Id).Count())
            {
                throw new ArgumentException("Cycle contains duplicate nodes.");
            }
        }
        catch (ArgumentException e)
        {
            System.Console.WriteLine("Invalid cycle detected");
            PrintPath(cycle);
            throw e;
        }
    }
    /// <summary>
    /// Combines two cycles into one. Given two simple cycles find a new longer cycle that visits all nodes from both cycles.
    /// </summary>
    /// <returns>True if combination is successful else false</returns>
    public static bool CombineCycles<TNode, TEdge>(this IGraph<TNode, TEdge> graph, IList<TNode> cycle1, IList<TNode> cycle2, out IList<TNode> result)
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        var intersection = cycle1.Select(x=>x.Id).Intersect(cycle2.Select(y=>y.Id)).ToArray();

        result = new List<TNode>(cycle1.Count+cycle2.Count-intersection.Length);
        
        if(intersection.Length<2)
            return false;

        var dict = new Dictionary<int,IList<TNode>>();
        foreach(var c1 in cycle1){
            dict[c1.Id] = new List<TNode>();
        }
        foreach(var c1 in cycle2){
            dict[c1.Id] = new List<TNode>();
        }
        cycle1.Aggregate((n1,n2)=>{
            dict[n1.Id].Add(n2);
            return n2;
        });
        cycle2.Aggregate((n1,n2)=>{
            dict[n1.Id].Add(n2);
            return n2;
        });
        
        foreach(var node in dict.Keys){
            var edges = dict[node];
            if(intersection.Contains(node))
            foreach(var e in edges.ToArray()){
                if(intersection.Contains(e.Id) && edges.Count>1)
                    edges.Remove(e);
            }
        }
        foreach(var m in dict){
            if(m.Value.Count!=1) return false;
        }
        var tmpNode = dict.First().Key;
        result.Add(graph.Nodes[tmpNode]);
        while(true){
            tmpNode = dict[tmpNode].First().Id;
            result.Add(graph.Nodes[tmpNode]);
            if(result.First().Id==result.Last().Id) break;
        }
        return true;
    }
    /// <summary>
    /// Prints path in a console.
    /// </summary>
    public static void PrintPath<TNode>(IList<TNode> path)
    where TNode : INode
    {
        System.Console.WriteLine("-------------------");
        foreach (var p in path)
        {
            System.Console.WriteLine(p);
        }
    }
    /// <summary>
    /// Checks if graph colored in a right way. Throws an exception if there is a case when some node is not colored in a right way.
    /// </summary>
    public static void EnsureRightColoring<TNode, TEdge>(this IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        foreach (var n in graph.Nodes)
        {
            var color = n.Color;
            var edges = graph.Edges[n.Id];
            if (edges.Any(x => x.Target.Color == color))
            {
                throw new WrongGraphColoringException($"Wrong graph coloring! Node {n.Id} with color {color} have edge with the same color!");
            }
        }
    }
    public static float MeanNodeEdgesCount<TNode, TEdge>(this IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge<TNode>
        => (float)(graph.Edges.Count) / (graph.Nodes.Count == 0 ? 1 : graph.Nodes.Count);

    /// <summary>
    /// Apply predicate on nodes and returns selected nodes Id as int array. Just a shortcut for convenience.
    /// </summary>
    public static int[] GetNodesIdWhere<TNode, TEdge>(this IGraph<TNode, TEdge> graph, Predicate<TNode> predicate)
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        return graph.Nodes.Where(x => predicate(x)).Select(x => x.Id).ToArray();
    }
}