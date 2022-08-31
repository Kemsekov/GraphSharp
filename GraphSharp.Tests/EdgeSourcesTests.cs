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
    public void Check_OutEdgesInEdgesIntegrity()
    {
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            _Graph.CheckForIntegrityOfSimpleGraph();
            for (int i = 0; i < 100; i++)
            {
                var source = Random.Shared.Next(1000);
                var target = Random.Shared.Next(1000);
                try
                {
                    edgeSource.Remove(source, target);
                }
                catch (Exception) { }
                _Graph.CheckForIntegrityOfSimpleGraph();
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
            _Graph.CheckForIntegrityOfSimpleGraph();
        }
    }
    [Fact]
    public void Neighbors_Works(){
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            _Graph.Do.ConnectRandomly(2,10);
            foreach(var n in Nodes){
                var outNeighbors = _Graph.Edges.OutEdges(n.Id).Select(x=>x.TargetId);
                var inNeighbors = _Graph.Edges.InEdges(n.Id).Select(x=>x.SourceId);
                var expected = outNeighbors.Union(inNeighbors);
                var actual = _Graph.Edges.Neighbors(n.Id);
                Assert.Equal(expected,actual);
            }
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
            _Graph.CheckForIntegrityOfSimpleGraph();
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
            _Graph.CheckForIntegrityOfSimpleGraph();

            var e1 = new Edge(Nodes[0], Nodes[5]){Weight=1};
            var e2 = new Edge(Nodes[0], Nodes[5]){Weight=2};
            edgeSource.Add(e1);
            edgeSource.Add(e2);
            edgeSource.Remove(e1);
            Assert.Equal(edgeSource[0,5].Weight,e2.Weight);
            edgeSource.Add(e1);
            edgeSource.Remove(0,5);
            Assert.Empty(edgeSource.GetParallelEdges(0,5));

            edgeSource.Add(e1);
            edgeSource.Add(e1);
            edgeSource.Add(e1);
            Assert.Equal(edgeSource.GetParallelEdges(0,5).Count(),3);
            edgeSource.Remove(e1);
            Assert.Equal(edgeSource.GetParallelEdges(0,5).Count(),0);


        }
    }
    [Fact]
    public void GetBothEdges_Works(){
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            for(int i = 0;i<100;i++){
                (var outEdges, var inEdges) = edgeSource.BothEdges(i);
                Assert.Equal(inEdges,edgeSource.InEdges(i));
                Assert.Equal(outEdges,edgeSource.OutEdges(i));
            }
        }
    }
    [Fact]
    public void GetParallelEdges_Works(){
        foreach (var edgeSource in EdgeSources)
        {
            var e1 = new Edge(Nodes[0], Nodes[5]){Weight=1};
            var e2 = new Edge(Nodes[0], Nodes[5]){Weight=2};
            var e3 = new Edge(Nodes[0], Nodes[5]){Weight=3};

            edgeSource.Add(e1);
            edgeSource.Add(e2);
            edgeSource.Add(e3);

            Assert.Equal(edgeSource.GetParallelEdges(0,5).Select(x=>x.Weight),new[]{e1,e2,e3}.Select(x=>x.Weight));
            edgeSource.Remove(e3);
            Assert.Equal(edgeSource.GetParallelEdges(0,5),new[]{e1,e2});
        }
    }
    [Fact]
    public void Contains_Works(){
        foreach (var edgeSource in EdgeSources)
        {
            var e1 = new Edge(Nodes[0], Nodes[5]){Weight=1};
            var e2 = new Edge(Nodes[0], Nodes[5]){Weight=2};
            edgeSource.Add(e1);
            edgeSource.Add(e2);
            Assert.True(edgeSource.Contains(0,5));
            edgeSource.Remove(e2);
            Assert.True(edgeSource.Contains(0,5));
            Assert.False(edgeSource.Contains(e2));
            edgeSource.Remove(e1);

            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            foreach(var e in edgeSource){
                Assert.True(edgeSource.Contains(e));
                Assert.True(edgeSource.Contains(e.SourceId,e.TargetId));
            }
        }
    }
    [Fact]
    public void Degree_Works(){
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            for(var i = 0;i<1000;i++){
                var expected = edgeSource.OutEdges(i).Count()+edgeSource.InEdges(i).Count();
                var actual = edgeSource.Degree(i);
                Assert.Equal(expected,actual);
            }
        }
    }
    [Fact]
    public void IsSink_And_IsSources_Works(){
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            FillEdges(Nodes, edgeSource, 1000);
            _Graph.Do.MakeUndirected();
            _Graph.Do.MakeSources(1,2,3,4);
            
            Assert.True(edgeSource.IsSource(1));
            Assert.True(edgeSource.IsSource(2));
            Assert.True(edgeSource.IsSource(3));
            Assert.True(edgeSource.IsSource(4));

            for(int i = 5;i<1000;i++){
                if(edgeSource.Degree(i)!=0){
                    Assert.False(edgeSource.IsSource(i));
                }
            }

            _Graph.Do.ReverseEdges();

            Assert.True(edgeSource.IsSink(1));
            Assert.True(edgeSource.IsSink(2));
            Assert.True(edgeSource.IsSink(3));
            Assert.True(edgeSource.IsSink(4));

            for(int i = 5;i<1000;i++){
                if(edgeSource.Degree(i)!=0){
                    Assert.False(edgeSource.IsSink(i));
                }
            }
        }
    }

    [Fact]
    public void IsIsolated_Works(){
        foreach (var edgeSource in EdgeSources)
        {
            _Graph.SetSources(Nodes, edgeSource);
            _Graph.Do.ConnectToClosest(0,3);
            var isolated = _Graph.GetNodesIdWhere(x=>edgeSource.Degree(x.Id)==0);
            var nonIsolated = Nodes.Select(x=>x.Id).Except(isolated);
            foreach(var i in isolated){
                Assert.True(edgeSource.IsIsolated(i));
            }
            foreach(var i in nonIsolated){
                Assert.False(edgeSource.IsIsolated(i));
            }
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
                Assert.NotEmpty(edgeSource.OutEdges(edge.SourceId));
                Assert.Contains((edge.SourceId, edge.TargetId), edgeSource.OutEdges(edge.SourceId).Select(x => (x.SourceId, x.TargetId)));
                var _ = edgeSource[edge.SourceId, edge.TargetId];
            }
            Assert.Empty(edgeSource.OutEdges(-100));
            Assert.Empty(edgeSource.OutEdges(12300));
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
            _Graph.CheckForIntegrityOfSimpleGraph();
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
                _Graph.CheckForIntegrityOfSimpleGraph();

                //and count of edge must keep the same
                Assert.Equal(edgeSource.Count, edgesCount);
            }
        }
    }
}