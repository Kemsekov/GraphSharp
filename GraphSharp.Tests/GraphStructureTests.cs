using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Tests.Helpers;
using GraphSharp.Tests.Models;
using Xunit;

namespace GraphSharp.Tests
{
    public class GraphStructureTests
    {

        private int _nodes_count;
        private GraphStructure<TestNode,TestEdge> _GraphStructure;

        public GraphStructureTests()
        {
            this._nodes_count = 500;
            this._GraphStructure = new GraphStructure<TestNode,TestEdge>(id=>new TestNode(id),(n,p)=>new TestEdge(n)){
                Distance = (n1, n2) => n1.Id - n2.Id
            }.CreateNodes(_nodes_count);
        }
        [Fact]
        public void ConnectToClosestWorks()
        {
            _GraphStructure.ForEach()
            .ConnectToClosest(1,6);
            validateThereIsNoCopiesAndParentInEdges(_GraphStructure.Nodes);
        }
        [Fact]
        public void MakeDirectedWorks()
        {
            //create two identical nodes list
            var seed = new Random().Next();
            var directed =
                new GraphStructure<TestNode,TestEdge>(id=>new(id),(n,_)=>new(n),rand: new Random(seed))
                    .CreateNodes(2000)
                    .ForEach()
                    .ConnectNodes(20)
                    .MakeDirected();
            var undirected =
                new GraphStructure<TestNode,TestEdge>(id=>new(id),(n,_)=>new(n),rand: new Random(seed))
                    .CreateNodes(2000)
                    .ForEach()
                    .ConnectNodes(20);

            Assert.Equal(directed.Nodes, undirected.Nodes);

            //make sure each child have no connection to parent
            foreach (var parent in directed.Nodes)
            {
                foreach (var child in parent.Edges)
                {
                    Assert.False(child.Node.Edges.Any(c => c.Node.Id == parent.Id));
                }
            }

            //make sure we did not remove anything that is not connected to node
            foreach (var parents in directed.Nodes.Zip(undirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());
                var directedEdges = parents.First.Edges.Select(x => x.Node);
                var undirectedEdges = parents.Second.Edges.Select(x => x.Node);
                var diff = undirectedEdges.Except(directedEdges, new NodeEqualityComparer());

                foreach (var n in diff.Select(x=>x as TestNode))
                {
                    Assert.True(n.Edges.Any(x => x.Node.Id == parents.First.Id));
                }
            }

        }
        [Fact]
        public void MakeUndirectedWorks()
        {
            var seed = new Random().Next();
            var maybeUndirected =
                new GraphStructure<TestNode,TestEdge>(id=>new(id),(n,_)=>new(n),rand: new Random(seed))
                .CreateNodes(2000)
                .ForEach()
                .ConnectNodes(20);

            var undirected =
                new GraphStructure<TestNode,TestEdge>(id=>new(id),(n,_)=>new(n),rand: new Random(seed))
                .CreateNodes(2000)
                .ForEach()
                .ConnectNodes(20)
                //one of them make 100% undirected
                .MakeUndirected();

            //ensure they are the same
            Assert.Equal(maybeUndirected.Nodes, undirected.Nodes);

            //make sure each child have connection to parent
            foreach (var parent in undirected.Nodes)
            {
                foreach (var child in parent.Edges)
                {
                    Assert.True(child.Node.Edges.Any(c => c.Node.Id == parent.Id));
                }
            }

            //make sure we did not add anything redundant
            foreach (var parents in undirected.Nodes.Zip(maybeUndirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());

                var undirectedEdges = parents.First.Edges.Select(x => x.Node);
                var maybeUndirectedEdges = parents.Second.Edges.Select(x => x.Node);

                var diff = maybeUndirectedEdges.Except(undirectedEdges, new NodeEqualityComparer());
                Assert.Empty(diff);

                diff = undirectedEdges.Except(maybeUndirectedEdges, new NodeEqualityComparer());

                foreach (var n in diff)
                {
                    Assert.True(maybeUndirected.Nodes[n.Id].Edges.Any(x => x.Node.Id == parents.First.Id));
                }
            }
        }
        [Fact]
        public void EnsureNodesCount()
        {
            Assert.Equal(_GraphStructure.Nodes.Count, _nodes_count);
        }
        [Fact]
        public void ConnectNodesWorks()
        {
            int children_count = 100;
            _GraphStructure.ForEach()
            .ConnectNodes(children_count);
            validateThereIsNoCopiesAndParentInEdges(_GraphStructure.Nodes);
            ensureRightCountOfEdgesPerNode(_GraphStructure.Nodes, 100, 100);
            Parallel.ForEach(_GraphStructure.Nodes, node =>
             {
                 var edges = node.Edges.Select(child => child.Node).ToList();
                 Assert.Equal(node.Edges.Count, children_count);
                 validateThereIsNoCopiesAndParentInEdges(edges);
             });
        }
        [Fact]
        public void ConnectRandomlyWorks()
        {
            const int minCountOfNodes = 5;
            const int maxCountOfNodes = 30;
            _GraphStructure.ForEach()
            .ConnectRandomly(minCountOfNodes, maxCountOfNodes);

            validateThereIsNoCopiesAndParentInEdges(_GraphStructure.Nodes);
            ensureRightCountOfEdgesPerNode(_GraphStructure.Nodes, minCountOfNodes, maxCountOfNodes);

            Parallel.ForEach(_GraphStructure.Nodes, node =>
             {
                 var edges = node.Edges.Select(child => child.Node).ToList();
                 ensureRightCountOfEdgesPerNode(_GraphStructure.Nodes, minCountOfNodes, maxCountOfNodes);
                 validateThereIsNoCopiesAndParentInEdges(edges);
             });
        }
        [Fact]
        public void ClearWorkingGroup_Works()
        {
            var nodes = _GraphStructure
                .ForNodes(x=>x.Where(n=>n.Id%2==0))
                .ConnectNodes(10)
                .ClearWorkingGroup()
                .ForNodes(x=>x.Where(n=>n.Edges.Count==0))
                .ConnectNodes(5)
                .Nodes;
            foreach(var n in nodes.Where(n=>n.Id%2==0))
                Assert.Equal(n.Edges.Count,10);
            foreach(var n in nodes.Where(n=>n.Id%2!=0))
                Assert.Equal(n.Edges.Count,5);
        }
        public void validateThereIsNoCopiesAndParentInEdges(IEnumerable<TestNode> nodes)
        {
            Assert.NotEmpty(nodes);
            foreach (var parent in nodes)
            {
                Assert.Equal(parent.Edges.Distinct(), parent.Edges);
                Assert.False(parent.Edges.Any(child => child.Node.Id == parent.Id), $"There is parent in children. Parent : {parent}");
            }
        }
        public void ensureRightCountOfEdgesPerNode(IEnumerable<TestNode> nodes, int minEdges, int maxEdges)
        {
            Assert.NotEmpty(nodes);
            foreach (var node in nodes)
            {
                var edgesCount = node.Edges.Count();
                Assert.True(edgesCount >= minEdges && edgesCount <= maxEdges,$"{edgesCount} >= {minEdges} && {edgesCount} <= {maxEdges}");
            }
        }


    }
}