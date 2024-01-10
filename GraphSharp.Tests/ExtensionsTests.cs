using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;
using Xunit;

namespace GraphSharp.Tests;
public class ExtensionsTests
{
    /// <summary>
    /// Creates an automorphism of graph, producing two isomorphic graphs
    /// </summary>
    /// <returns>new graph that is isomorphic to input graph and mapping of original nodes to new graph</returns>
    public static (Graph isomorphic, Dictionary<int, int> mapping) CreateAutomorphism<TNode,TEdge>(IGraph<TNode,TEdge> g)
    where TNode : INode
    where TEdge : IEdge
    {
        var sourceNodes = g.Nodes.Select(n=>n.Id).ToArray();
        var mapped = sourceNodes.OrderBy(i=>Random.Shared.Next()).ToArray();
        var mapping = sourceNodes.Zip(mapped).ToDictionary(k=>k.First,k=>k.Second);

        var isomorphic = new Graph();
        foreach(var n in g.Nodes){
            isomorphic.Nodes.Add(new Node(mapping[n.Id]));
        }
        foreach(var e in g.Edges){
            var source = mapping[e.SourceId];
            var target = mapping[e.TargetId];
            isomorphic.Edges.Add(new Edge(source,target));
        }
        return (isomorphic,mapping);
    }
}