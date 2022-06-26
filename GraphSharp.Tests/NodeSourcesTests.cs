using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.GraphStructures;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests
{
    public class NodeSourcesTests
    {
        IEnumerable<INodeSource<TestNode>> NodeSources;
        IEnumerable<IEdgeSource<TestNode,TestEdge>> EdgeSources;

        public NodeSourcesTests()
        {
            NodeSources = new List<INodeSource<TestNode>>()
            {
                new DefaultNodeSource<TestNode>(0)
            };
            EdgeSources = new List<IEdgeSource<TestNode,TestEdge>>()
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

        [Fact]
        public void When_Empty_MaxMinNodeId_Is_Minus_One(){
            foreach(var nodeSource in NodeSources){
                Assert.Equal(nodeSource.MaxNodeId,-1);
                Assert.Equal(nodeSource.MinNodeId,-1);
            }
        }
        [Fact]
        public void Count_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);
                Assert.Equal(1000, nodeSource.Count);
                Assert.Equal(999,nodeSource.MaxNodeId);
                Assert.Equal(0,nodeSource.MinNodeId);
                var counter = 0;
                foreach(var n in nodeSource){
                    Assert.Equal(n.Id,counter++);
                }
            }
        }
        [Fact]
        public void Add_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);

                Assert.Equal(nodeSource.MaxNodeId,999);

                nodeSource.Add(new TestNode(1010));
                Assert.Equal(nodeSource[1010].Id, 1010);
                Assert.Equal(nodeSource.Count,1001);
                Assert.Equal(nodeSource.MaxNodeId,1010);

                nodeSource.Add(new TestNode(-1));
                Assert.Equal(nodeSource[-1].Id, -1);
                Assert.Equal(nodeSource.Count,1002);
                Assert.Equal(nodeSource.MinNodeId,-1);
            }
        }
        [Fact]
        public void Remove_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);

                nodeSource.Remove(500);
                nodeSource.Remove(999);
                Assert.Equal(nodeSource.MaxNodeId,998);
                nodeSource.Remove(0);
                Assert.Equal(nodeSource.MinNodeId,1);
                Assert.Equal(nodeSource.Count,997);
            }
        }
        [Fact]
        public void GetEnumerator_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);

                var counter = 0;
                foreach(var n in nodeSource){
                    Assert.Equal(n.Id,counter++);
                }
            }
        }
        [Fact]
        public void TryGetNode_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);

                Assert.True(nodeSource.TryGetNode(500,out var n1));
                Assert.False(nodeSource.TryGetNode(1500,out var n2));
                Assert.False(nodeSource.TryGetNode(-100,out var n3));
                Assert.True(nodeSource.TryGetNode(0,out var n4));
                Assert.True(nodeSource.TryGetNode(999,out var n5));
            }
        }
        [Fact]
        public void Clear_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);

                nodeSource.Clear();
                Assert.Equal(nodeSource.MaxNodeId,-1);
                Assert.Equal(nodeSource.MinNodeId,-1);
                Assert.Equal(nodeSource.Count,0);
                foreach(var n in nodeSource){
                    Assert.True(false);
                }
            }
        }
        [Fact]
        public void RandomAccess_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);

                Assert.Equal(nodeSource[500].Id,500);
                Assert.Equal(nodeSource[999].Id,999);
                Assert.Equal(nodeSource[0].Id,0);
                Assert.Throws<KeyNotFoundException>(()=>nodeSource[-1]);
                Assert.Throws<KeyNotFoundException>(()=>nodeSource[12345]);
            }
        }
    }
}