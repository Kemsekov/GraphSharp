using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GraphSharp.Common;
using GraphSharp.Exceptions;
namespace GraphSharp.Graphs;

/// <summary>
/// Contains extensions for <see cref="IGraph{,}"/> and <see cref="IImmutableGraph{,}"/>
/// </summary>
public static class GraphExtensions
{
    /// <returns>
    /// Other part of the edge.<br/> 
    /// If <paramref name="nodeId"/> equals to 
    /// <paramref name="SourceId"/> then returns <paramref name="TargetId"/>.<br/>
    /// If <paramref name="nodeId"/> equals to 
    /// <paramref name="TargetId"/> then returns <paramref name="SourceId"/>.<br/>
    /// If none returns -1
    /// </returns>
    public static int Other<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph, TEdge edge, int nodeId)
    where TNode : INode
    where TEdge : IEdge
    {
        if(edge.SourceId==nodeId)
            return edge.TargetId;
        if(edge.TargetId==nodeId)
            return edge.SourceId;
        return -1;
    }
    /// <returns>True if given graph have one single component</returns>
    public static bool IsConnected<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        using var c = graph.Do.FindComponents();
        return c.Components.Length == 1;
    }
    /// <summary>
    /// Clears current Nodes and Edges with new ones. Does not clear old Nodes and Edges.
    /// </summary>
    public static IGraph<TNode, TEdge> Clear<TNode, TEdge>(this IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        graph.Nodes.Clear();
        graph.Edges.Clear();
        return graph;
    }
    /// <summary>
    /// Clones graph structure
    /// </summary>
    /// <returns>Copy of current graph structure</returns>
    public static IGraph<TNode, TEdge> Clone<TNode, TEdge>(this IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        var result = graph.CloneJustConfiguration();
        foreach (var n in graph.Nodes)
            graph.CloneNodeTo(n, result.Nodes);
        foreach (var e in graph.Edges)
            graph.CloneEdgeTo(e, result.Edges);
        return result;
    }
    /// <summary>
    /// Creates a graph structure of same configuration that that currently used. 
    /// Do not clone any nodes or edges.
    /// </summary>
    /// <returns>Cloned by config graph structure</returns>
    public static IGraph<TNode, TEdge> CloneJustConfiguration<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        var result = new Graph<TNode, TEdge>(graph.Configuration);
        return result;
    }
    /// <returns>True if given graph have one single strongly connected component</returns>
    public static bool IsStronglyConnected<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        using var componentsResult = graph.Do.FindStronglyConnectedComponentsTarjan();
        return componentsResult.Components.Count() == 1;
    }
    /// <returns>True if graph is directed, else false</returns>
    public static bool IsDirected<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        foreach (var e in graph.Edges)
        {
            if (graph.Edges.TryGetEdge(e.TargetId, e.SourceId, out _))
            {
                return false;
            }
        }
        return true;
    }
    /// <returns>True if graph is bidirected, else false</returns>
    public static bool IsBidirected<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        foreach (var e in graph.Edges)
        {
            if (!graph.Edges.TryGetEdge(e.TargetId, e.SourceId, out _))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Determine whatever given graph is a directed tree
    /// </summary>
    /// <param name="graph"></param>
    /// <returns>True if directed tree</returns>
    public static bool IsDirectedTree<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        if (graph.IsConnected())
        {
            return graph.Edges.Count() + 1 == graph.Nodes.Count();
        }
        return false;
    }
    /// <summary>
    /// Checks for data integrity for Nodes and Edges for the case when current graph is simple. <br/>
    /// </summary>
    /// <exception cref="GraphDataIntegrityException">When there is a problem with data integrity in a graph. See exception message for more details.</exception>
    public static void CheckForIntegrityOfSimpleGraph<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        GraphDataIntegrityChecker.CheckForEdgeIndicesIntegrity(graph);
        GraphDataIntegrityChecker.CheckForEdgesIndexDuplicates(graph);
        GraphDataIntegrityChecker.CheckForNodeIndicesIntegrity(graph);
        GraphDataIntegrityChecker.CheckForNodesDuplicates(graph);
    }
    /// <summary>
    /// Validates that given path is a valid path for current graph.
    /// </summary>
    public static void ValidatePath<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph, IList<TNode> path)
    where TNode : INode
    where TEdge : IEdge
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
    public static void ValidateCycle<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph, IList<TNode> cycle)
    where TNode : INode
    where TEdge : IEdge
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
    /// Combines two cycles into one. 
    /// Given two cycles find a longer cycle that visits all nodes from both cycles. 
    /// Can be used to determine whatever two given cycles are simple to each other.
    /// If it can combine two cycles into one(returns true) it means given cycles are not simple to
    /// each other, if it cannot - they simple to each other.
    /// </summary>
    /// <returns>True if combination is successful else false</returns>
    public static bool CombineCycles<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph, IList<TNode> cycle1, IList<TNode> cycle2, out IList<TNode> result)
    where TNode : INode
    where TEdge : IEdge
    {
        var intersection = cycle1.Select(x => x.Id).Intersect(cycle2.Select(y => y.Id)).ToArray();

        var cycles = cycle1.Concat(cycle2);
        result = new List<TNode>(cycle1.Count + cycle2.Count - intersection.Length);

        if (intersection.Length < 2)
            return false;

        var dict = new Dictionary<int, IList<TNode>>();
        var incomingEdges = new Dictionary<int, int>();
        foreach (var c1 in cycles)
        {
            dict[c1.Id] = new List<TNode>();
            incomingEdges[c1.Id] = 0;
        }
        cycles.Aggregate((n1, n2) =>
        {
            dict[n1.Id].Add(n2);
            incomingEdges[n2.Id]++;
            return n2;
        });


        foreach (var node in dict.Keys)
        {
            var edges = dict[node];
            if (intersection.Contains(node))
                foreach (var e in edges.ToArray())
                {
                    if (intersection.Contains(e.Id) && edges.Count > 1 && incomingEdges[e.Id] > 1)
                    {
                        edges.Remove(e);
                        incomingEdges[e.Id]--;
                    }
                }
        }

        //check if two cycles merged successfully
        foreach (var m in dict)
            if (m.Value.Count != 1) return false;
        foreach (var m in incomingEdges)
            if (m.Value != 1) return false;

        var tmpNode = dict.First().Key;
        result.Add(graph.Nodes[tmpNode]);
        while (true)
        {
            tmpNode = dict[tmpNode].First().Id;
            result.Add(graph.Nodes[tmpNode]);
            if (result.First().Id == result.Last().Id) break;
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
    /// Computes path cost on enumerable of node ids using cost function
    /// </summary>
    public static double ComputePathCost<TNode,TEdge>(this IImmutableGraph<TNode,TEdge> graph,IEnumerable<int> path,Func<TEdge,double>? getCost = null)
    where TNode : INode
    where TEdge : IEdge
    {
        getCost ??= edge=>edge.Weight;
        var cost = 0d;
        path.Aggregate((n1,n2)=>{
            cost += getCost(graph.Edges[n1,n2]);
            return n2;
        });
        return cost;
    }
    /// <summary>
    /// Computes path cost on enumerable of node using cost function
    /// </summary>
    public static double ComputePathCost<TNode,TEdge>(this IImmutableGraph<TNode,TEdge> graph,IEnumerable<TNode> path,Func<TEdge,double>? getCost = null)
    where TNode : INode
    where TEdge : IEdge
    {
        getCost ??= edge=>edge.Weight;
        var cost = 0d;
        path.Aggregate((n1,n2)=>{
            cost += getCost(graph.Edges[n1.Id,n2.Id]);
            return n2;
        });
        return cost;
    }
    /// <summary>
    /// Computes path cost on enumerable of path edges
    /// </summary>
    public static double ComputePathCost<TNode,TEdge>(this IImmutableGraph<TNode,TEdge> graph,IEnumerable<TEdge> path,Func<TEdge,double>? getWeight = null)
    where TNode : INode
    where TEdge : IEdge
    {
        getWeight ??= edge=>edge.Weight;
        return path.Sum(x=>getWeight(x));
    }
    /// <summary>
    /// Checks if graph colored in a right way. Throws an exception if there is a case when some node is not colored in a right way.
    /// </summary>
    public static void EnsureRightColoring<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        var Nodes = graph.Nodes;
        foreach (var n in Nodes)
        {
            var color = n.Color;
            var edges = graph.Edges.OutEdges(n.Id);
            if (edges.Any(x => Nodes[x.TargetId].Color == color))
            {
                throw new WrongGraphColoringException($"Wrong graph coloring! Node {n.Id} with color {color} have edge with the same color!");
            }
        }
    }
    /// <summary>
    /// Apply predicate on nodes and returns selected nodes Id as int array. Just a shortcut for convenience.
    /// </summary>
    public static int[] GetNodesIdWhere<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph, Predicate<TNode> predicate)
    where TNode : INode
    where TEdge : IEdge
    {
        return graph.Nodes.Where(x => predicate(x)).Select(x => x.Id).ToArray();
    }
    /// <summary>
    /// Clones <paramref name="edge"/> to <paramref name="destination"/>
    /// </summary>
    /// <param name="edge">Edge to clone</param>
    /// <param name="destination">Edges source that will accept cloned edge</param>
    /// <returns>Clone of <paramref name="edge"/></returns>
    public static TEdge CloneEdgeTo<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> src, TEdge edge, IEdgeSource<TEdge> destination)
            where TNode : INode
            where TEdge : IEdge
    {
        var clonedEdge = (TEdge)edge.Clone();
        destination.Add(clonedEdge);
        return clonedEdge;
    }
    /// <summary>
    /// Clones <paramref name="node"/> to <paramref name="destination"/>
    /// </summary>
    /// <param name="edge">Edge to clone</param>
    /// <param name="destination">Nodes source that will accept cloned node</param>
    /// <returns>Clone of <paramref name="node"/></returns>
    public static TNode CloneNodeTo<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph, TNode node, INodeSource<TNode> destination)
    where TNode : INode
    where TEdge : IEdge
    {
        var newNode = (TNode)node.Clone();
        destination.Add(newNode);
        return newNode;
    }
    /// <summary>
    /// Sets new nodes and edges to a graph. Will enumerate every enumerable and add it's values to a graph.
    /// </summary>
    public static IGraph<TNode, TEdge> SetSources<TNode, TEdge>(this IGraph<TNode, TEdge> graph, IEnumerable<TNode>? nodes = null, IEnumerable<TEdge>? edges = null)
    where TNode : INode
    where TEdge : IEdge
    {
        nodes ??= graph.Nodes;
        edges ??= graph.Edges;
        graph.SetSources(new DefaultNodeSource<TNode>(nodes),new DefaultEdgeSource<TEdge>(edges));
        return graph;
    }
    /// <summary>
    /// Set some color to all nodes
    /// </summary>
    public static void SetColorToAll<TNode>(this IImmutableNodeSource<TNode> nodes, System.Drawing.Color color)
    where TNode : INode
    {
        foreach (var n in nodes)
        {
            n.Color = color;
        }
    }
    /// <summary>
    /// Set some color to all edges
    /// </summary>
    public static void SetColorToAll<TEdge>(this IImmutableEdgeSource<TEdge> edges, System.Drawing.Color color)
    where TEdge : IEdge
    {
        foreach (var n in edges)
        {
            n.Color = color;
        }
    }
    /// <summary>
    /// Converts edges list to path (nodes list)
    /// </summary>
    /// <returns>Converted path</returns>
    public static IList<TNode> ConvertEdgesListToPath<TNode, TEdge>(this IImmutableGraph<TNode, TEdge> graph, IList<TEdge> edges)
    where TNode : INode
    where TEdge : IEdge
    {
        if (edges.Count == 0) return new List<TNode>();
        var m = edges.MaxBy(x => Math.Max(x.SourceId, x.TargetId)) ?? throw new Exception();
        var nodesCount = Math.Max(m.SourceId, m.TargetId);
        using var addedNodes = ArrayPoolStorage.RentByteArray(nodesCount + 1);
        var edgesSource = new DefaultEdgeSource<TEdge>(edges);
        var expectedNodesCount = edges.Count + 1;
        int sink = -1;
        foreach (var e in edges)
        {
            if (edgesSource.InEdges(e.SourceId).Count() == 0)
            {
                sink = e.SourceId;
            }
        }
        if (sink == -1)
        {
            sink = edges.First().SourceId;
        }
        var result = new List<int>(expectedNodesCount);
        result.Add(sink);

        var next = edgesSource.OutEdges(result.Last());
        while (true)
        {
            next = edgesSource.OutEdges(result.Last());
            if (next.Count() != 1) break;
            var toAdd = next.First().TargetId;
            addedNodes[toAdd] += 1;
            if (addedNodes[toAdd] > 1)
                throw new ArgumentException("Given edges list is not a path. Some edges touch the same node twice");

            result.Add(toAdd);
            if (result.Count == edges.Count + 1) break;
        }
        if (result.Count != expectedNodesCount)
            throw new ArgumentException("Given edges list is not a path");
        return result.Select(x => graph.Nodes[x]).ToList();
    }
    // TODO: add test
    /// <summary>
    /// Converts nodes list that forms a path to edges they contains
    /// </summary>
    /// <returns><see langword="true"/> if converted successfully, else <see langword="false"/></returns>
    public static bool ConvertPathToEdges<TNode,TEdge>(this IImmutableGraph<TNode, TEdge> graph,  IList<TNode> nodes, out IList<TEdge> edges)
    where TNode : INode
    where TEdge : IEdge
    {
        var edges1 = new List<TEdge>();
        bool failed = false;
        nodes.Aggregate((n1,n2)=>{
            if(!failed) 
            if(graph.Edges.TryGetEdge(n1.Id,n2.Id,out var edge) && edge is not null){
                edges1.Add(edge);
            }
            else
                failed = true;
            return n2;
        });
        edges = edges1;
        return !failed;
    }
    /// <summary>
    /// Method to get source of the edge
    /// </summary>
    public static TNode GetSource<TNode,TEdge>(this IImmutableGraph<TNode,TEdge> graph, TEdge edge)
    where TNode : INode
    where TEdge : IEdge
    {
        return graph.Nodes[edge.SourceId];
    }
    /// <summary>
    /// Method to get target of the edge
    /// </summary>
    public static TNode GetTarget<TNode,TEdge>(this IImmutableGraph<TNode,TEdge> graph, TEdge edge)
    where TNode : INode
    where TEdge : IEdge
    {
        return graph.Nodes[edge.TargetId];
    }
}