using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Common;

using GraphSharp.Graphs;
using GraphSharp.Propagators;
using GraphSharp.Tests.Helpers;
using GraphSharp.Tests.Models;
using GraphSharp.Visitors;
using Xunit;

namespace GraphSharp.Tests;
public class GraphTests
{

    private IGraph<Node, Edge> _Graph;

    public GraphTests()
    {
        this._Graph = new Graph<Node, Edge>(new TestGraphConfiguration(new Random()));
        _Graph.Do.CreateNodes(1000);
    }
    void CheckClique(int nodeId,CliqueResult clique){
        Assert.Equal(clique.InitialNodeId,nodeId);
        Assert.Contains(nodeId,clique.Nodes);
        var induced = _Graph.Do.Induce(clique.Nodes);
        induced.Do.MakeBidirected();
        var complement = induced.Do.GetComplement();
        Assert.True(complement.All(x=>x.SourceId==x.TargetId));
        Assert.Equal(complement.Count,clique.Nodes.Count);
    }
    [Fact]
    public void FindClique_Works(){
        _Graph.Do.CreateNodes(500);
        _Graph.Do.ConnectNodes(10);
        foreach(var n in _Graph.Nodes){
            var clique = _Graph.Do.FindClique(n.Id);
            CheckClique(n.Id,clique);
        }
    }
    [Fact]
    public void FindCliqueFast_Works(){
        _Graph.Do.CreateNodes(500);
        _Graph.Do.ConnectNodes(10);
        foreach(var n in _Graph.Nodes){
            var clique = _Graph.Do.FindCliqueFast(n.Id);
            CheckClique(n.Id,clique);
        }
    }
    [Fact]
    public void FindMaxClique_Works(){
        _Graph.Do.CreateNodes(500);
        _Graph.Do.ConnectNodes(10);
        var clique = _Graph.Do.FindMaxClique();
        CheckClique(clique.InitialNodeId,clique);
    }
    [Fact]
    public void FindMaxCliqueFast_Works(){
        _Graph.Do.CreateNodes(500);
        _Graph.Do.ConnectNodes(10);
        var clique = _Graph.Do.FindMaxCliqueFast();
        CheckClique(clique.InitialNodeId,clique);
    }

    [Fact]
    public void MaxFlow_Works(){
        _Graph.Do.CreateNodes(500);
        _Graph.Do.ConnectNodes(10);
        _Graph.Do.MakeSources(0);
        var clone = _Graph.Clone();
        var sink = _Graph.Nodes.First(x=>_Graph.Edges.IsSink(x.Id)).Id;
        var flow = _Graph.Do.MaxFlowEdmondsKarp(0,sink,x=>Random.Shared.NextDouble());
        Assert.True(flow.MaxFlow>0);

        //check that after max flow we didn't changed our graph
        Assert.Equal(_Graph.Edges.Count,clone.Edges.Count);
        Assert.Equal(_Graph.Nodes.Count,clone.Nodes.Count);
        foreach(var e in clone.Edges){
            Assert.True(_Graph.Edges.Contains(e.SourceId,e.TargetId));
        }
        foreach(var e in _Graph.Edges){
            Assert.True(clone.Edges.Contains(e.SourceId,e.TargetId));
        }
    }
    [Fact]
    public void TryFindHamiltonianCycleByBubbleExpansion_Works()
    {
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.DelaunayTriangulation(x=>x.MapProperties().Position);
        _Graph.Do.MakeBidirected();
        var result = _Graph.Do.TryFindHamiltonianCycleByBubbleExpansion();
        _Graph.ValidateCycle(result);
    }
    [Fact]
    public void DelaunayTriangulation_Works(){
        _Graph.Do.CreateNodes(500);
        foreach(var n in _Graph.Nodes){
            n.Properties["position"] = new double[]{Random.Shared.NextDouble(),Random.Shared.NextDouble(),Random.Shared.NextDouble()};
        }
        _Graph.Do.DelaunayTriangulation(n=>(double[])n.Properties["position"]);
        foreach(var n in _Graph.Nodes){
            Assert.NotEmpty(_Graph.Edges.AdjacentEdges(n.Id));
        }
    }
    [Fact]
    public void TopologicalSort_Works()
    {
        _Graph.Do.CreateNodes(500);
        _Graph.Do.ConnectRandomly(3, 6)
                 .MakeBidirected();
        var startPositions = new int[] { 1, 2, 3, 4 };
        TestTopologicalSort(startPositions);
        _Graph.Do.CreateNodes(500);
        _Graph.Do.ConnectRandomly(3, 6)
                 .MakeSources(5, 6, 7);
        TestTopologicalSort();

    }
    void TestTopologicalSort(params int[] startPositions)
    {

        var t1 = _Graph.Do.TopologicalSort(startPositions);
        foreach (var l in t1.Layers)
        {
            Assert.Equal(l.Count, l.Distinct().Count());
        }
        var diff = t1.Layers.SelectMany(x => x).Except(_Graph.Nodes.Select(n=>n.Id));
        Assert.Empty(diff);
        Assert.Equal(t1.Layers.Sum(x => x.Count), _Graph.Nodes.Count);
        var layer = t1.Layers.GetEnumerator();
        int visitedCount = 0;
        var visited = new byte[_Graph.Nodes.MaxNodeId + 1];
        var visitor = new ActionVisitor<Node, Edge>(
            visit: node =>
            {
                Assert.True(layer.Current.Remove(node));
                visited[node] = 1;
                visitedCount++;
            },
            select: edge => visited[edge.TargetId] == 0,
            start: () => layer.MoveNext()
        );
        var propagator = new Propagator<Edge>(_Graph.Edges,visitor);
        if (startPositions.Length != 0)
            propagator.SetPosition(startPositions);
        else
            propagator.SetPosition(_Graph.GetNodesIdWhere(x => _Graph.Edges.IsSource(x.Id)));
        while (visitedCount != _Graph.Nodes.Count)
        {
            propagator.Propagate();
        }
        Assert.Equal(visitedCount, _Graph.Nodes.Count);
        Assert.True(t1.Layers.All(x => x.Count == 0));

    }
    [Fact]
    public void TryFindCenter_Works()
    {
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.DelaunayTriangulation(x=>x.MapProperties().Position);
        (var r1, var c1) = _Graph.Do.TryFindCenterByApproximation(x => 1,false);
        (var r2, var c2) = _Graph.Do.FindCenterByDijkstras(x => 1,false);
        Assert.NotEmpty(c1);
        Assert.NotEmpty(c2);
        c2 = c2.OrderBy(x=>x.Id).ToList();
        c1 = c1.OrderBy(x=>x.Id).ToList();
        Assert.Equal(c2.ToHashSet(), c1.ToHashSet());
        foreach (var c in c1.Concat(c2))
        {
            var ecc = _Graph.Do.FindEccentricity(c.Id, x => 1).length;
            Assert.Equal(ecc, r1);
        }
    }
    [Fact]
    public void FindLocalClusteringCoefficients_Works()
    {
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.ConnectRandomly(2, 10);
        var coeffs = _Graph.Do.FindLocalClusteringCoefficients();
        Assert.True(coeffs.All(x => x <= 1f && x >= 0f));
        foreach (var n in _Graph.Nodes)
        {
            var neighbors = _Graph.Edges.Neighbors(n.Id).ToArray();
            var degree = _Graph.Edges.Degree(n.Id);
            double inducedEdgesCount = _Graph.Do.Induce(neighbors).Edges.Count + degree;
            var nodesCount = 1 + neighbors.Count();
            Assert.Equal(coeffs[n.Id], inducedEdgesCount / (nodesCount * (nodesCount - 1)));
        }
    }
    [Fact]
    public void GetComplement_Works()
    {
        var complement = _Graph
            .Do.CreateNodes(100)
            .ConnectRandomly(1, 5)
            .GetComplement();
        foreach (var c in complement)
        {
            bool contains = _Graph.Edges.Contains(c.SourceId, c.TargetId);
            Assert.False(contains);
        }
        var n = _Graph.Nodes.Count;
        var e1 = _Graph.Edges.Count;
        var e2 = complement.Count-n;
        Assert.Equal(n*(n-1),e1+e2);
    }
    [Fact]
    public void FindStronglyConnectedComponents_Works()
    {
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.ConnectToClosest(2, 5,(n1,n2)=>(n1.MapProperties().Position-n2.MapProperties().Position).L2Norm());
        var ssc = _Graph.Do.FindStronglyConnectedComponentsTarjan();
        Assert.NotEmpty(ssc.Components);
        foreach (var c in ssc.Components)
        {
            Assert.NotEmpty(c.nodes);
            foreach (var n1 in c.nodes)
            {
                foreach (var n2 in c.nodes)
                {
                    if (n1.Equals(n2)) continue;
                    var path = _Graph.Do.FindAnyPath(n1.Id, n2.Id).Path;
                    Assert.NotEmpty(path);
                }
            }
        }
    }
    [Fact]
    public void FindEccentricity_Works()
    {
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.DelaunayTriangulation(x=>x.MapProperties().Position);
        var node = _Graph.Nodes[Random.Shared.Next(1000)];
        var ecc = _Graph.Do.FindEccentricity(node.Id);
        var paths = _Graph.Do.FindShortestPathsDijkstra(node.Id);
        var foundEcc = paths.PathLength.MaxBy(x => x);
        Assert.Equal(ecc.length, foundEcc);
    }
    [Fact]
    public void FindCycleBasis_Works()
    {
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.ConnectRandomly(0, 7);
        var tree = _Graph.Do.FindSpanningForestKruskal().Forest;
        var cycles = _Graph.Do.FindCyclesBasis();
        foreach (var c in cycles)
        {
            int outEdgesCount = 0;
            _Graph.ValidateCycle(c);
            c.Path.Aggregate((n1, n2) =>
            {
                outEdgesCount += tree.Where(x => x.SourceId == n1.Id && x.TargetId == n2.Id).Count();
                return n2;
            });
            Assert.Equal(c.Path.Count - outEdgesCount, 2);
        }
        foreach (var c1 in cycles)
            foreach (var c2 in cycles)
            {
                if (c1.Equals(c2)) continue;
                Assert.False(_Graph.CombineCycles(c1, c2, out var _));
            }
    }

    [Fact]
    public void FindSpanningTree_Works()
    {
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.ConnectRandomly(0, 7);
        var result = _Graph.Do.FindComponents();
        var tree = _Graph.Do.FindSpanningForestKruskal().Forest;

        UnionFind u = new(_Graph.Nodes.MaxNodeId + 1);
        foreach (var n in _Graph.Nodes)
            u.MakeSet(n.Id);
        foreach (var e in tree)
        {
            Assert.NotEqual(u.FindSet(e.SourceId), u.FindSet(e.TargetId));
            u.UnionSet(e.SourceId, e.TargetId);
        }
        foreach (var n1 in _Graph.Nodes)
        {
            foreach (var n2 in _Graph.Nodes)
            {
                if (result.SetFinder.FindSet(n1.Id) == result.SetFinder.FindSet(n2.Id))
                    Assert.Equal(u.FindSet(n1.Id), u.FindSet(n2.Id));
            }
        }
        var edgeSource = _Graph.Configuration.CreateEdgeSource();
        foreach (var e in tree)
        {
            edgeSource.Add(e);
        }
        _Graph.SetSources(_Graph.Nodes, edgeSource);
        Assert.True(_Graph.IsDirected());
    }
    [Fact]
    public void FindArticulationPoints_Works()
    {
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.ConnectRandomly(0, 7);
        var before = _Graph.Do.FindComponents().Components.Count();
        var after = 0;
        var points = _Graph.Do.FindArticulationPointsTarjan().Select(x => x.Id);
        Assert.Equal(points, points.Distinct());
        foreach (var p in points)
        {
            _Graph.Do.RemoveNodes(p);
            after = _Graph.Do.FindComponents().Components.Count();
            Assert.True(after >= before, $"{after}>{before}");
            before = after;
        }
    }

    [Fact]
    public void FindComponents_Works()
    {
        _Graph.Do.ConnectRandomly(0, 6);
        int indexer = 0;
        var result = _Graph.Do.FindComponents();
        Assert.Equal(result.Components.Length,result.SetFinder.SetsCount);
        var indexedComponents = result.Components.Select(x => (indexer++, x)).ToArray();
        var paired = new Dictionary<(int, int), int>();
        foreach (var c1 in indexedComponents)
        {
            foreach (var c2 in indexedComponents)
            {
                if (c1 == c2) continue;
                if (!paired.TryGetValue((c1.Item1, c2.Item1), out var _))
                    paired[(c1.Item1, c2.Item1)] = 1;

                foreach (var n1 in c1.x)
                    foreach (var n2 in c2.x)
                    {
                        var path = _Graph.Do.FindAnyPath(n1.Id, n2.Id).Path;
                        Assert.Empty(path);
                        Assert.NotEqual(result.SetFinder.FindSet(n1.Id), result.SetFinder.FindSet(n2.Id));
                    }
            }
        }
    }

    [Fact]
    public void IsDirected_Works()
    {
        var seed = new Random().Next();
        var directed = _Graph;

        directed.Do
            .CreateNodes(1000)
            .ConnectNodes(20)
            .MakeDirected();
        Assert.True(directed.IsDirected());
    }

    [Fact]
    public void IsDirectedTree_Works()
    {
        _Graph.Do.CreateNodes(1000).ConnectNodes(10);
        var tree = _Graph.Do.FindSpanningForestKruskal();
        Assert.False(_Graph.IsDirectedTree());
        _Graph.SetSources(_Graph.Nodes, new DefaultEdgeSource<Edge>(tree.Forest));
        Assert.True(_Graph.IsDirectedTree());
        _Graph.Edges.Remove(_Graph.Edges.First());
        Assert.False(_Graph.IsDirectedTree());
    }

    [Fact]
    public void IsBidirected_Works()
    {
        var seed = new Random().Next();
        var directed = _Graph;
        directed.Do
            .CreateNodes(1000)
            .ConnectNodes(20)
            .MakeBidirected();
        Assert.True(directed.IsBidirected());
    }
    
    [Fact]
    public void Clone_Works()
    {
        _Graph.Do.ConnectRandomly(1, 5);
        var clone = _Graph.Clone();
        Assert.Equal(_Graph.Nodes, clone.Nodes);
        Assert.Equal(_Graph.Edges.Count, clone.Edges.Count);
        var t1 = _Graph.Converter.ToConnectionsList();
        var t2 = clone.Converter.ToConnectionsList();
        Assert.Equal(t1, t2);
        clone.Do.RemoveEdges(x => x.TargetId % 2 == 0);
        t1 = _Graph.Converter.ToConnectionsList();
        t2 = clone.Converter.ToConnectionsList();
        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void Induce_Works()
    {
        _Graph.Do.ConnectRandomly(1, 5);
        var toInduce = _Graph.GetNodesIdWhere(x => x.Id % 3 == 0);
        var induced = _Graph.Do.Induce(toInduce);
        induced.CheckForIntegrityOfSimpleGraph();

        foreach (var n in induced.Nodes)
        {
            Assert.True(n.Id % 3 == 0);
        }
        foreach (var e in induced.Edges)
        {
            Assert.True(e.SourceId % 3 == 0 || e.TargetId % 3 == 0);
        }

        var node_diff = _Graph.Nodes.Select(x => x.Id).Except(induced.Nodes.Select(x => x.Id));
        foreach (var id in node_diff)
        {
            Assert.True(id % 3 != 0);
        }

        var edge_diff = _Graph.Edges.Select(x => (x.SourceId, x.TargetId)).Except(induced.Edges.Select(x => (x.SourceId, x.TargetId)));
        foreach (var e in edge_diff)
        {
            Assert.True(e.Item1 % 3 != 0 || e.Item2 % 3 != 0);
        }
    }

    [Fact]
    public void CombineCycles_Works()
    {
        {
            var cycle1 =_Graph.ToPath(new Node[] { new(26), new(90), new(86), new(89), new(26) },PathType.OutEdges);
            var cycle2 =_Graph.ToPath(new Node[] { new(86), new(26), new(94), new(90), new(86) },PathType.OutEdges);

            Assert.True(_Graph.CombineCycles(cycle1, cycle2, out var combined));
            Assert.True(combined.Count > cycle1.Count && combined.Count > cycle2.Count);
        }
        {
            var cycle1 = _Graph.ToPath(new Node[] { new(1), new(8), new(7), new(9), new(2), new(1) },PathType.OutEdges);
            var cycle2 = _Graph.ToPath(new Node[] { new(3), new(2), new(9), new(7), new(6), new(5), new(4), new(3) },PathType.OutEdges);
            Assert.False(_Graph.CombineCycles(cycle1, cycle2, out var combined));
        }
        _Graph.Do.CreateNodes(1000);
        _Graph.Do.ConnectNodes(10);
        var cycles = _Graph.Do.FindCyclesBasis();
        var accumulator = new List<(IList<Node> cycle1, IList<Node> cycle2)>();
        foreach (var c1 in cycles)
        {
            var possibleCycles = cycles.Where(c2 => c2.Intersect(c1).Count() > 1).ToArray();
            if (possibleCycles.Length > 2)
                accumulator.Add((c1, possibleCycles[1]));
        }
        var inCycles = new byte[_Graph.Nodes.MaxNodeId];
        foreach ((var cycle1, var cycle2) in accumulator)
        {
            Array.Fill(inCycles, (byte)0);
            foreach (var c in cycle1.Concat(cycle2))
                inCycles[c.Id] = 1;
            var c1 = _Graph.ToPath(cycle1,PathType.OutEdges);
            var c2 = _Graph.ToPath(cycle2,PathType.OutEdges);
            if (_Graph.CombineCycles(c1, c2, out var combined))
            {
                _Graph.ValidateCycle(combined);
                Assert.True(combined.Count >= cycle1.Count && combined.Count >= cycle2.Count);
                Assert.True(combined.All(n => inCycles[n.Id] == 1));
            }
        }
    }

}