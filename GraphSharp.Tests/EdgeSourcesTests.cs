using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Exceptions;
using GraphSharp.GraphStructures;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests
{
    public class EdgeSourcesTests
    {
        INodeSource<TestNode> Nodes;
        IEnumerable<IEdgeSource<TestEdge>> EdgeSources;

        public EdgeSourcesTests()
        {
            Nodes = new DefaultNodeSource<TestNode>(0);
            Fill(Nodes,1000);
            EdgeSources = new List<IEdgeSource<TestEdge>>()
            {
                new DefaultEdgeSource<TestNode,TestEdge>()
            };
        }
        void Fill(INodeSource<TestNode> Nodes, int nodesCount){
            for (int i = 0; i < nodesCount; i++)
            {
                var node = new TestNode(i);
                Nodes.Add(node);
            }
        }

        void FillEdges(INodeSource<TestNode> nodes, IEdgeSource<TestEdge> edges, int edgesCount){
            int nodesCount = nodes.Count;
            for(int i=0;i<edgesCount;i++){
                var source = nodes[Random.Shared.Next(nodesCount)];
                var target = nodes[Random.Shared.Next(nodesCount)];
                var edge = new TestEdge(source,target);
                edges.Add(edge);
            }
        }

        [Fact]
        public void Add_Works(){
            foreach(var edgeSource in EdgeSources){
                FillEdges(Nodes,edgeSource,1000);
                Assert.Equal(1000,edgeSource.Count);
            }
        }
        [Fact]
        public void Count_Works(){
            foreach(var edgeSource in EdgeSources){
                FillEdges(Nodes,edgeSource,1000);
                Assert.Equal(1000,edgeSource.Count);
                edgeSource.Remove(edgeSource.First());
                edgeSource.Remove(edgeSource.First());
                Assert.Equal(998,edgeSource.Count);
                edgeSource.Remove(new TestEdge(new TestNode(10000),new TestNode(10010)));
                Assert.Equal(998,edgeSource.Count);
            }
        }
        [Fact]
        public void Remove_Works(){
            foreach(var edgeSource in EdgeSources){
                edgeSource.Add(new TestEdge(Nodes[0],Nodes[1]));
                edgeSource.Add(new TestEdge(Nodes[0],Nodes[2]));
                Assert.True(edgeSource.Remove(0,1));
                Assert.Equal(edgeSource.Count,1);
                Assert.False(edgeSource.Remove(5,5));
                Assert.Equal(edgeSource.Count,1);
                Assert.True(edgeSource.Remove(edgeSource.First()));
                Assert.Equal(edgeSource.Count,0);
            }
        }
        [Fact]
        public void RandomAccess_Works(){
            foreach(var edgeSource in EdgeSources){
                FillEdges(Nodes,edgeSource,1000);
                var edges = edgeSource.Take(100).ToArray();
                foreach(var edge in edges){
                    Assert.NotEmpty(edgeSource[edge.Source.Id]);
                    Assert.Contains((edge.Source.Id,edge.Target.Id),edgeSource[edge.Source.Id].Select(x=>(x.Source.Id,x.Target.Id)));
                    var _ = edgeSource[edge.Source.Id,edge.Target.Id];
                }
                Assert.Empty(edgeSource[-100]);
                Assert.Empty(edgeSource[12300]);
                Assert.Throws<EdgeNotFoundException>(()=>edgeSource[1234,1235]);
            }
        }
        [Fact]
        public void TryGetEdge_Works(){
            foreach(var edgeSource in EdgeSources){
                FillEdges(Nodes,edgeSource,1000);
                var edges = edgeSource.Take(100).ToArray();
                foreach(var edge in edges){
                    Assert.True(edgeSource.TryGetEdge(edge.Source.Id,edge.Target.Id,out var _found));
                    Assert.Equal(edge,_found);
                }
                Assert.False(edgeSource.TryGetEdge(-100,100,out var found));
                Assert.Null(found);
                Assert.False(edgeSource.TryGetEdge(1234,1235,out found));
                Assert.Null(found);
            }
        }
        [Fact]
        public void Clear_Works(){
            foreach(var edgeSource in EdgeSources){
                FillEdges(Nodes,edgeSource,1000);
                edgeSource.Clear();
                Assert.Equal(0,edgeSource.Count);
                foreach(var edge in edgeSource){
                    Assert.False(true);
                }
            }
        }
    }
}