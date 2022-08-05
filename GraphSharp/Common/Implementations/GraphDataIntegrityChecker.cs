using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GraphSharp.Exceptions;
using GraphSharp.Graphs;


namespace GraphSharp.Common;
public static class GraphDataIntegrityChecker
{
    public static void CheckForNodesDuplicates<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        var actual = graph.Nodes.Select(x => x.Id);
        var expected = actual.Distinct();
        if (actual.Count() != expected.Count())
            throw new GraphDataIntegrityException("Nodes contains duplicates");
    }
    public static void CheckForNodeIndicesIntegrity<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        foreach(var n in graph.Nodes){
            var index = n.Id;
            var value = graph.Nodes[n.Id].Id;
            if(index!=value)
                throw new GraphDataIntegrityException($"Node {n.Id} have wrong index! At index {index} reside {value}. {index}!={value}");
        }
    }
    public static void CheckForEdgeIndicesIntegrity<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        foreach (var n in graph.Nodes)
        {
            var edges = graph.Edges[n.Id];
            if (!edges.All(x => x.SourceId == n.Id))
            {
                throw new GraphDataIntegrityException($"Edges list for node {n.Id} has edge which source Id is not {n.Id}");
            }
        }
        foreach(var expected in graph.Edges){
            var actual = graph.Edges[expected.SourceId,expected.TargetId];
            if(actual.SourceId!=expected.SourceId || actual.TargetId!=expected.TargetId)
                    throw new GraphDataIntegrityException($"Edges index returned wrong value! At index {(expected.SourceId,expected.TargetId)} reside edge with index {(actual.SourceId,actual.TargetId)}");
        }
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
        foreach (var n in graph.Nodes)
        {
            foreach (var source in graph.Edges.GetSourcesId(n.Id))
            {
                if (!graph.Edges.TryGetEdge(source, n.Id, out var _))
                    throw new GraphDataIntegrityException($"Edge {source}->{n.Id} is present in sources list but not found among edges");
            }
        }
    }
    public static void CheckForEdgesDuplicates<TNode, TEdge>(IGraph<TNode, TEdge> graph)
    where TNode : INode
    where TEdge : IEdge
    {
        foreach (var n in graph.Nodes){
            var edges = graph.Edges[n.Id];
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
}