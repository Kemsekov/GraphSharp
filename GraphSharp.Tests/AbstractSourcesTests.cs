using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.GraphStructures;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests
{
    public class AbstractSourcesTests
    {
        IEnumerable<INodeSource<TestNode>> NodeSources;
        IEnumerable<IEdgeSource<TestEdge>> EdgeSources;

        public GraphStructure<TestNode, TestEdge> Graph { get; }

        public AbstractSourcesTests()
        {
            NodeSources = new List<INodeSource<TestNode>>()
            {
                new DefaultNodeSource<TestNode>(0)
            };
            EdgeSources = new List<IEdgeSource<TestEdge>>()
            {
                new DefaultEdgeSource<TestNode,TestEdge>()
            };
            Graph = new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(new Random()));
        }
        [Fact]
        public void NodeSource_Count_Works(){
            foreach(var nodeSource in NodeSources){
                Graph.SetSources(nodeSource, EdgeSources.First());
                Graph.Create(1000);
                Assert.Equal(1000, nodeSource.Count);
                Assert.Equal(999,nodeSource.MaxNodeId);
                Assert.Equal(0,nodeSource.MinNodeId);
                var counter = 0;
                foreach(var n in nodeSource){
                    Assert.Equal(n.Id,counter++);
                }
            }
        }
    }
}