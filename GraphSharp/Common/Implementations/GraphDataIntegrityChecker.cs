using System.Linq;
using System.Text;
using GraphSharp.Exceptions;
using GraphSharp.Graphs;
namespace GraphSharp.Common;

/// <summary>
/// Contains methods to check integrity of graph data structure
/// </summary>
public static class GraphDataIntegrityChecker
{
    /// <summary>
    /// Seeks for nodes duplicates in a graph. 
    /// </summary>
    /// <exception cref="GraphDataIntegrityException">If found duplicates</exception>
    public static void CheckForNodesDuplicates<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        var actual = graph.Nodes.Select(x => x.Id);
        var expected = actual.Distinct();
        if (actual.Count() != expected.Count())
            throw new GraphDataIntegrityException("Nodes contains duplicates");
    }
    /// <summary>
    /// Checks that each node retrieved by index have <paramref name="Id"/> equal to that same index.
    /// </summary>
    /// <exception cref="GraphDataIntegrityException">If node at some index don't have same Id</exception>
    public static void CheckForNodeIndicesIntegrity<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        foreach (var n in graph.Nodes)
        {
            var index = n.Id;
            var value = graph.Nodes[n.Id].Id;
            if (index != value)
                throw new GraphDataIntegrityException($"Node {n.Id} have wrong index! At index {index} reside {value}. {index}!={value}");
        }
    }
    /// <summary>
    /// Checks that each edge received by <paramref name="SourceId"/> and <paramref name="TargetId"/> is in fact have the same <paramref name="SourceId"/> and <paramref name="TargetId"/> <br/>
    /// Checks if every nodeId from each edge is present in nodes (there is no ghost links) <br/>
    /// Check that in and out edges are consistent with each other
    /// </summary>
    /// <exception cref="GraphDataIntegrityException">If there is problems with edges integrity</exception>
    public static void CheckForEdgeIndicesIntegrity<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        // check retrieve out edges and in edges 
        foreach (var n in graph.Nodes)
        {
            var edges = graph.Edges.OutEdges(n.Id);
            if (!edges.All(x => x.SourceId == n.Id))
            {
                throw new GraphDataIntegrityException($"Out edges list for node {n.Id} has edge which source Id is not {n.Id}");
            }
            edges = graph.Edges.InEdges(n.Id);
            if (!edges.All(x => x.TargetId == n.Id))
            {
                throw new GraphDataIntegrityException($"In edges list for node {n.Id} has edge which target Id is not {n.Id}");
            }
        }

        // check retrieve index
        foreach (var expected in graph.Edges)
        {
            var actual = graph.Edges[expected.SourceId, expected.TargetId];
            if (actual.SourceId != expected.SourceId || actual.TargetId != expected.TargetId)
                throw new GraphDataIntegrityException($"Edges index returned wrong value! At index {(expected.SourceId, expected.TargetId)} reside edge with index {(actual.SourceId, actual.TargetId)}");
        }
        // check for ghost nodes among edges
        foreach (var e in graph.Edges)
        {
            if (!graph.Nodes.TryGetNode(e.SourceId, out var _))
            {
                throw new GraphDataIntegrityException($"{e.SourceId} found among Edges but not found among Nodes");
            }
            if (!graph.Nodes.TryGetNode(e.TargetId, out var _))
            {
                throw new GraphDataIntegrityException($"{e.TargetId} found among Edges but not found among Nodes");
            }
        }
        // check that in and out edges are consistent with each other
        foreach (var e in graph.Edges)
        {
            if (!graph.Edges.OutEdges(e.SourceId).Contains(e))
            {
                throw new GraphDataIntegrityException($"{e} is not found among out edges of {e.SourceId}");
            }
            if (!graph.Edges.InEdges(e.TargetId).Contains(e)){
                throw new GraphDataIntegrityException($"{e} is not found among in edges of {e.TargetId}");
            }
        }
    }
    /// <summary>
    /// Checks for index duplicates among edges. Will throw if sourceId -> targetId is not unique. Throw if there is any parallel edges.
    /// </summary>
    /// <exception cref="GraphDataIntegrityException">If there is duplicates among edges</exception>
    public static void CheckForEdgesIndexDuplicates<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        foreach (var n in graph.Nodes)
        {
            var edges = graph.Edges.OutEdges(n.Id);
            var actualEdges = edges.Select(x => (x.SourceId, x.TargetId));
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
    }
    /// <summary>
    /// Checks for edges duplicates using <paramref name="Equals"/> of edge
    /// </summary>
    /// <exception cref="GraphDataIntegrityException">If there is duplicates among edges</exception>
    public static void CheckForEdgesDuplicates<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        foreach (var n in graph.Nodes)
        {
            var edges = graph.Edges.OutEdges(n.Id);
            var actualEdges = edges;
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
    }
}