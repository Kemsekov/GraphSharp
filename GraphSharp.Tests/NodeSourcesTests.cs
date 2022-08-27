using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Graphs;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests
{
    public class NodeSourcesTests
    {
        IEnumerable<INodeSource<Node>> NodeSources;
        IEnumerable<IEdgeSource<Edge>> EdgeSources;

        public NodeSourcesTests()
        {
            NodeSources = new List<INodeSource<Node>>()
            {
                new DefaultNodeSource<Node>()
            };
            EdgeSources = new List<IEdgeSource<Edge>>()
            {
                new DefaultEdgeSource<Edge>()
            };
        }

        void Fill(INodeSource<Node> Nodes, int nodesCount){
            for (int i = 0; i < nodesCount; i++)
            {
                var node = new Node(i);
                Nodes.Add(node);
            }
        }

        void CheckForIntegrity(INodeSource<Node> n){
            var g = new Graph();
            g.SetSources(n,new DefaultEdgeSource<Edge>());
            g.CheckForIntegrityOfSimpleGraph();
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
                CheckForIntegrity(nodeSource);
            }
        }
        [Fact]
        public void Add_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);

                Assert.Equal(nodeSource.MaxNodeId,999);

                nodeSource.Add(new Node(1010));
                Assert.Equal(nodeSource[1010].Id, 1010);
                Assert.Equal(nodeSource.Count,1001);
                Assert.Equal(nodeSource.MaxNodeId,1010);

                nodeSource.Add(new Node(-1));
                Assert.Equal(nodeSource[-1].Id, -1);
                Assert.Equal(nodeSource.Count,1002);
                Assert.Equal(nodeSource.MinNodeId,-1);
                CheckForIntegrity(nodeSource);
                nodeSource.Clear();
                for(int i = 0;i<1000;i++){
                    nodeSource.Add(new Node(i));
                    Assert.True(nodeSource.TryGetNode(i,out var _));
                }
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
                CheckForIntegrity(nodeSource);
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
                CheckForIntegrity(nodeSource);

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
        [Fact]
        public void Move_Works(){
            foreach(var nodeSource in NodeSources){
                Fill(nodeSource,1000);
                for(int i = 0;i<100;i++){
                    var id1 = Random.Shared.Next(1000);
                    var id2 = Random.Shared.Next(2000);
                    if(!nodeSource.TryGetNode(id2,out var _)){
                        if(!nodeSource.TryGetNode(id1,out var _)) continue;
                        var n1 = nodeSource[id1];
                        Assert.True(nodeSource.Move(id1,id2));
                        Assert.False(nodeSource.TryGetNode(id1,out var _));
                        Assert.True(nodeSource.TryGetNode(id2,out var _));
                    }
                    else{
                        Assert.False(nodeSource.Move(id1,id2));
                    }
                    CheckForIntegrity(nodeSource);
                    Assert.Equal(nodeSource.Count,1000);
                }

            }
        }
    }
}