using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Exceptions;
using GraphSharp.Graphs;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests;
public class EdgeSourcesTests
{
    private Graph<Node, Edge> _Graph;
    INodeSource<Node> Nodes;
    IEnumerable<IEdgeSource<Edge>> EdgeSources;
    const int NodesCount = 1000;
    public EdgeSourcesTests()
    {
        this._Graph = new Graph<Node, Edge>(new TestGraphConfiguration(new Random()));
        Nodes = new DefaultNodeSource<Node>();
        Fill(Nodes, NodesCount);
        EdgeSources = new List<IEdgeSource<Edge>>()
            {
                new DefaultEdgeSource<Edge>()
            };
    }

    void Fill(INodeSource<Node> Nodes, int nodesCount)
    {
        for (int i = 0; i < nodesCount; i++)
        {
            var node = new Node(i);
            Nodes.Add(node);
        }
    }

    void FillEdges(INodeSource<Node> nodes, IEdgeSource<Edge> edges, int edgesCount)
    {
        int nodesCount = nodes.Count;
        for (int i = 0; i < edgesCount; i++)
        {
            var source = nodes[Random.Shared.Next(nodesCount)];
            var target = nodes[Random.Shared.Next(nodesCount)];
            var edge = new Edge(source, target);
            if (!edges.TryGetEdge(source.Id, target.Id, out var _))
                edges.Add(edge);
            else
                i--;
        }
    }

    [Fact]
    public void GetSourcesId()
    {
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            _Graph.CheckForIntegrity();
            for (int i = 0; i < 100; i++)
            {
                var source = Random.Shared.Next(1000);
                var target = Random.Shared.Next(1000);
                try
                {
                    edgeSource.Remove(source, target);
                }
                catch (Exception) { }
                _Graph.CheckForIntegrity();
            }
        }
    }

    [Fact]
    public void Add_Works()
    {
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            Assert.Equal(1000, edgeSource.Count);
            _Graph.CheckForIntegrity();
        }
    }
    [Fact]
    public void Count_Works()
    {
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            Assert.Equal(1000, edgeSource.Count);
            edgeSource.Remove(edgeSource.First());
            edgeSource.Remove(edgeSource.First());
            Assert.Equal(998, edgeSource.Count);
            edgeSource.Remove(new Edge(new Node(10000), new Node(10010)));
            Assert.Equal(998, edgeSource.Count);
            foreach (var e in edgeSource.Take(100).ToArray())
            {
                edgeSource.Remove(e);
            }
            Assert.Equal(898, edgeSource.Count);
            _Graph.CheckForIntegrity();
        }
    }
    [Fact]
    public void Remove_Works()
    {
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            edgeSource.Add(new Edge(Nodes[0], Nodes[1]));
            edgeSource.Add(new Edge(Nodes[0], Nodes[2]));
            Assert.True(edgeSource.Remove(0, 1));
            Assert.Equal(edgeSource.Count, 1);
            Assert.False(edgeSource.Remove(5, 5));
            Assert.Equal(edgeSource.Count, 1);
            Assert.True(edgeSource.Remove(edgeSource.First()));
            Assert.Equal(edgeSource.Count, 0);

            _Graph.CheckForIntegrity();
        }
    }
    [Fact]
    public void RandomAccess_Works()
    {
        foreach (var edgeSource in EdgeSources)
        {
            FillEdges(Nodes, edgeSource, 1000);
            var edges = edgeSource.Take(100).ToArray();
            foreach (var edge in edges)
            {
                Assert.NotEmpty(edgeSource[edge.SourceId]);
                Assert.Contains((edge.SourceId, edge.TargetId), edgeSource[edge.SourceId].Select(x => (x.SourceId, x.TargetId)));
                var _ = edgeSource[edge.SourceId, edge.TargetId];
            }
            Assert.Empty(edgeSource[-100]);
            Assert.Empty(edgeSource[12300]);
            Assert.Throws<EdgeNotFoundException>(() => edgeSource[1234, 1235]);
        }
    }
    [Fact]
    public void TryGetEdge_Works()
    {
        foreach (var edgeSource in EdgeSources)
        {
            FillEdges(Nodes, edgeSource, 1000);
            var edges = edgeSource.Take(100).ToArray();
            foreach (var edge in edges)
            {
                Assert.True(edgeSource.TryGetEdge(edge.SourceId, edge.TargetId, out var _found));
                Assert.Equal(edge, _found);
            }
            Assert.False(edgeSource.TryGetEdge(-100, 100, out var found));
            Assert.Null(found);
            Assert.False(edgeSource.TryGetEdge(1234, 1235, out found));
            Assert.Null(found);
        }
    }
    [Fact]
    public void Clear_Works()
    {
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            edgeSource.Clear();
            Assert.Equal(0, edgeSource.Count);
            foreach (var edge in edgeSource)
            {
                Assert.False(true);
            }
            _Graph.CheckForIntegrity();
        }
    }
    [Fact]
    public void Move_Works()
    {
        var edgesCount = 2000;
        foreach (var edgeSource in EdgeSources)
        {
            FillEdges(Nodes, edgeSource, edgesCount);
            _Graph.SetSources(Nodes, edgeSource);
            for (int i = 0; i < 100; i++)
            {
                var e = edgeSource.ElementAt(i);
                var newSourceIndex = Random.Shared.Next(NodesCount);
                var newTargetIndex = Random.Shared.Next(NodesCount);
                var oldSourceIndex = e.SourceId;
                var oldTargetIndex = e.TargetId;
                //if we have a free space at that index
                if (!edgeSource.TryGetEdge(newSourceIndex, newTargetIndex, out var _))
                {
                    //then we must be able to move old edge
                    Assert.True(edgeSource.Move(e, newSourceIndex, newTargetIndex));
                    //and after it moved there must be moved edge
                    Assert.True(edgeSource.TryGetEdge(newSourceIndex, newTargetIndex, out var _));
                    //and must not be old edge
                    Assert.False(edgeSource.TryGetEdge(oldSourceIndex, oldTargetIndex, out var _));
                }
                else
                {
                    Assert.False(edgeSource.Move(e, newSourceIndex, newTargetIndex));
                    Assert.False(edgeSource.TryGetEdge(newSourceIndex, newTargetIndex, out var _));
                    Assert.True(edgeSource.TryGetEdge(oldSourceIndex, oldTargetIndex, out var _));
                }
                Assert.Equal((e.SourceId, e.TargetId), (newSourceIndex, newTargetIndex));

                //check that everything with edge data is OK
                _Graph.CheckForIntegrity();

                //and count of edge must keep the same
                Assert.Equal(edgeSource.Count, edgesCount);
            }
        }
    }
}