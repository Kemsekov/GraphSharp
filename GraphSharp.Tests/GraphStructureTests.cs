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
            this._GraphStructure = new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(){
                Rand = new Random()
            }).CreateNodes(_nodes_count);
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
                new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(){Rand = new(seed)})
                    .CreateNodes(2000);
            directed
                .ForEach()
                .ConnectNodes(20)
                .MakeDirected();
            var undirected =
                new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(){Rand = new(seed)})
                    .CreateNodes(2000);
            undirected
                .ForEach()
                .ConnectNodes(20);
            ensureDirected(directed,undirected);
        }
        [Fact]
        public void CreateSourcesWorks(){
            var graph = new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration());
            graph.Converter.FromConnectionsList(ManualTestData.TestConnectionsList.Select(x=>(x.parentId,x.children as IEnumerable<int>)));
            graph.ForEach().MakeUndirected();
            graph.ForEach().CreateSources(1,14);
            var expected = ManualTestData.AfterMakeSourcesExpected;
            var actual = graph.Converter.ToConnectionsList();
            Assert.NotEmpty(actual);
            Assert.Equal(expected.Count(),actual.Count());
            foreach(var e in expected){
                var toCompare = actual.First(x=>x.parent==e.parentId);
                Assert.Equal(e.children,toCompare.children);
            }
        }
        [Fact]
        public void MakeUndirectedWorks()
        {
            var seed = new Random().Next();
            var maybeUndirected =
                new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(){Rand = new(seed)})
                .CreateNodes(2000);
            maybeUndirected
                .ForEach()
                .ConnectNodes(20);

            var undirected =
                new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(){Rand = new(seed)})
                .CreateNodes(2000);
            undirected
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
                    Assert.True(child.Child.Edges.Any(c => c.Child.Id == parent.Id));
                }
            }

            //make sure we did not add anything redundant
            foreach (var parents in undirected.Nodes.Zip(maybeUndirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());

                var undirectedEdges = parents.First.Edges.Select(x => x.Child);
                var maybeUndirectedEdges = parents.Second.Edges.Select(x => x.Child);

                var diff = maybeUndirectedEdges.Except(undirectedEdges, new NodeEqualityComparer());
                Assert.Empty(diff);

                diff = undirectedEdges.Except(maybeUndirectedEdges, new NodeEqualityComparer());

                foreach (var n in diff)
                {
                    Assert.True(maybeUndirected.Nodes[n.Id].Edges.Any(x => x.Child.Id == parents.First.Id));
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
            ensureRightCountOfEdgesPerNode(_GraphStructure.Nodes, 100, 101);
            Parallel.ForEach(_GraphStructure.Nodes, node =>
             {
                 var edges = node.Edges.Select(child => child.Child).ToList();
                 Assert.Equal(node.Edges.Count, children_count);
                 validateThereIsNoCopiesAndParentInEdges(edges);
             });
        }
        [Fact]
        public void ConnectRandomlyWorks()
        {
            int minCountOfNodes = Random.Shared.Next(5);
            int maxCountOfNodes = Random.Shared.Next(5)+20;
            _GraphStructure.ForEach()
            .ConnectRandomly(minCountOfNodes, maxCountOfNodes);

            validateThereIsNoCopiesAndParentInEdges(_GraphStructure.Nodes);
            ensureRightCountOfEdgesPerNode(_GraphStructure.Nodes, minCountOfNodes, maxCountOfNodes);

            Parallel.ForEach(_GraphStructure.Nodes, node =>
            {
                var edges = node.Edges.Select(child => child.Child).ToList();
                validateThereIsNoCopiesAndParentInEdges(edges);
            });
        }
        [Fact]
        public void ClearWorkingGroup_Works()
        {
            _GraphStructure
                .ForNodes(x=>x.Where(n=>n.Id%2==0))
                .ConnectNodes(10);
            _GraphStructure
                .ClearWorkingGroup();
            _GraphStructure
                .ForNodes(x=>x.Where(n=>n.Edges.Count==0))
                .ConnectNodes(5);
            var nodes = _GraphStructure.Nodes;
            
            foreach(var n in nodes.Where(n=>n.Id%2==0))
                Assert.Equal(n.Edges.Count,10);
            foreach(var n in nodes.Where(n=>n.Id%2!=0))
                Assert.Equal(n.Edges.Count,5);
        }
        [Fact]
        public void TotalEdgesCount_Works(){
            int count = 0;
            _GraphStructure.ForEach().ConnectRandomly(0,5);
            foreach(var n in _GraphStructure.Nodes)
                count+=n.Edges.Count;
            Assert.Equal(count,_GraphStructure.EdgesCount());
        } 
        [Fact]
        public void MeanNodeEdgesCount_Works(){
            _GraphStructure.ForEach().ConnectRandomly(0,5);
            float expected = (float)(_GraphStructure.EdgesCount())/_GraphStructure.Nodes.Count();
            float actual = _GraphStructure.MeanNodeEdgesCount();
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void RemoveEdges_Works(){
            _GraphStructure.ForEach().ConnectRandomly(1,5);
            _GraphStructure.ForNodes(x=>x.Where(n=>n.Id%2==0)).RemoveEdges();
            foreach(var n in _GraphStructure.Nodes){
                if(n.Id%2==0)
                    Assert.Empty(n.Edges);
                foreach(var e in n.Edges){
                    Assert.True(e.Child.Id%2!=0);
                }
            }
        }
        [Fact]
        public void CountParents_Works(){
            _GraphStructure.ForEach().RemoveEdges();
            var parentsCount = _GraphStructure.CountParents();
            Assert.All(parentsCount,x=>Assert.Equal(x.Value,0));

            _GraphStructure.ForEach().ConnectRandomly(2,5);
            parentsCount = _GraphStructure.CountParents();

            foreach(var n in _GraphStructure.Nodes){
                foreach(var e in n.Edges){
                    parentsCount[e.Child.Id]--;
                }
            }

            Assert.All(parentsCount,x=>Assert.Equal(x.Value,0));
        }
        public void validateThereIsNoCopiesAndParentInEdges(IEnumerable<TestNode> nodes)
        {
            Assert.NotEmpty(nodes);
            foreach (var parent in nodes)
            {
                Assert.Equal(parent.Edges.Distinct(), parent.Edges);
                Assert.False(parent.Edges.Any(child => child.Child.Id == parent.Id), $"There is parent in children. Parent : {parent}");
            }
        }
        public void ensureRightCountOfEdgesPerNode(IEnumerable<TestNode> nodes, int minEdges, int maxEdges)
        {
            Assert.NotEmpty(nodes);
            foreach (var node in nodes)
            {
                var edgesCount = node.Edges.Count();
                Assert.True(edgesCount >= minEdges && edgesCount < maxEdges,$"{edgesCount} >= {minEdges} && {edgesCount} < {maxEdges}");
            }
        }
        public void ensureDirected(IGraphStructure<TestNode> directed,IGraphStructure<TestNode> undirected){

            Assert.Equal(directed.Nodes, undirected.Nodes);

            //make sure each child have no connection to parent
            foreach (var parent in directed.Nodes)
            {
                foreach (var child in parent.Edges)
                {
                    Assert.False(child.Child.Edges.Any(c => c.Child.Id == parent.Id));
                }
            }

            //make sure we did not remove anything that is not connected to node
            foreach (var parents in directed.Nodes.Zip(undirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());
                var directedEdges = parents.First.Edges.Select(x => x.Child);
                var undirectedEdges = parents.Second.Edges.Select(x => x.Child);
                var diff = undirectedEdges.Except(directedEdges, new NodeEqualityComparer());

                foreach (var n in diff.Select(x=>x as TestNode))
                {
                    Assert.True(n.Edges.Any(x => x.Child.Id == parents.First.Id));
                }
            }
        }

    }
}