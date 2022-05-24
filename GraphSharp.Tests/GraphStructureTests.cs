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
            this._GraphStructure = new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(new Random())).Create(_nodes_count);
        }
        [Fact]
        public void Reindex_Works(){
            _GraphStructure.Create(1000);
            _GraphStructure.Do.RemoveNodes(x=>x.Id%3==0);
            foreach(var n in _GraphStructure.Nodes){
                Assert.True(n.Id%3!=0);
            }
            var nodesCount = _GraphStructure.Nodes.Count;
            _GraphStructure.Do.Reindex();
            Assert.Equal(nodesCount,_GraphStructure.Nodes.Count);
            Assert.Equal(nodesCount-1,_GraphStructure.Nodes.MaxNodeId);
            Assert.Equal(0,_GraphStructure.Nodes.MinNodeId);

            int counter = 0;
            foreach(var n in _GraphStructure.Nodes){
                Assert.Equal(counter++,n.Id);
            }
            _GraphStructure.CheckForIntegrity();
        }
        [Fact]
        public void RemoveNodes_Works(){
            // TODO: implement test
            throw new NotImplementedException();
        }
        [Fact]
        public void Create_RightCountOfNodes(){
            _GraphStructure.Create(100);
            Assert.Equal(_GraphStructure.Nodes.Count,100);
            Assert.Equal(_GraphStructure.Nodes.MaxNodeId,99);
            Assert.Equal(_GraphStructure.Nodes.MinNodeId,0);
        }
        [Fact]
        public void ConnectToClosestWorks()
        {
            _GraphStructure.Do
            .ConnectToClosest(1,6);
            validateThereIsNoCopiesAndParentInEdges(_GraphStructure.Nodes,_GraphStructure.Edges);
        }
        [Fact]
        public void MakeDirectedWorks()
        {
            //create two identical nodes list
            var seed = new Random().Next();
            var directed =
                new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(new(seed)))
                    .Create(2000);
            directed
                .Do
                .ConnectNodes(20)
                .MakeDirected();
            
            var undirected =
                new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(new(seed)))
                    .Create(2000);
            undirected
                .Do
                .ConnectNodes(20);
            ensureDirected(directed,undirected);
        }
        [Fact]
        public void CreateSourcesWorks(){
            var graph = new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration());
            graph.Converter.FromConnectionsList(ManualTestData.TestConnectionsList);
            Assert.Equal(graph.Converter.ToConnectionsList(),ManualTestData.TestConnectionsList);
            graph.Do.MakeUndirected();

            graph.Do.CreateSources(1,14);
            var expected = ManualTestData.AfterMakeSourcesExpected;
            var actual = graph.Converter.ToConnectionsList();
            Assert.NotEmpty(actual);
            Assert.Equal(expected.Count(),actual.Count());
            foreach(var e in expected){
                var toCompare = actual.First(x=>x.Key==e.parentId);
                var exp = e.children.ToList();
                var act = toCompare.Value.ToList();
                exp.Sort();
                act.Sort();
                Assert.Equal(exp,act);
            }
        }
        [Fact]
        public void RemoveUndirectedEdgesWorks(){
            var graph = new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration());
            graph.Create(500);
            graph.Do.ConnectRandomly(0,8);
            var before_removal = graph.Converter.ToConnectionsList().ToList();
            graph.Do.RemoveUndirectedEdges();
            var after_removal = graph.Converter.ToConnectionsList().ToList();
            for(int parentId = 0;parentId<500;parentId++){
                var before = before_removal.FirstOrDefault(x=>x.Key==parentId);
                var after = after_removal.FirstOrDefault(x=>x.Key==parentId);
                if(before.Value is null) continue;                
                var diff = before.Value.Except(after.Value ?? Enumerable.Empty<int>());
                foreach(var nodeId in diff){
                    Assert.Contains(parentId,before_removal.First(x=>x.Key==nodeId).Value);
                }
            }
            //and concrete example

            graph.Converter.FromConnectionsList(
                new Dictionary<int,int[]>{
                    {0,new []{1,2,3,5}},
                    {1,new []{0,2}},
                    {2,new []{1,3,5}},
                    {3,new []{1,2,4}},
                    {4,new []{3,5}},
                    {5,new []{0,4}}
                }
            );
            graph.Do.RemoveUndirectedEdges();
            var expected = new[]{
                (0,new[]{2,3}),
                (2,new[]{5}),
                (3,new[]{1})
            };
            var actual = graph.Converter.ToConnectionsList().Select(x=>(x.Key,x.Value.ToArray()));
            Assert.Equal(expected,actual);
        }
        [Fact]
        public void MakeUndirectedWorks()
        {
            var seed = new Random().Next();
            var maybeUndirected =
                new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(){Rand = new(seed)})
                .Create(2000);
            maybeUndirected
                .Do
                .ConnectNodes(20);

            var undirected =
                new GraphStructure<TestNode,TestEdge>(new TestGraphConfiguration(){Rand = new(seed)})
                .Create(2000);
            undirected
                .Do
                .ConnectNodes(20)
                //one of them make 100% undirected
                .MakeUndirected();

            //ensure they are the same
            Assert.Equal(maybeUndirected.Nodes, undirected.Nodes);

            //make sure each child have connection to parent
            foreach (var parent in undirected.Nodes)
            {
                foreach (var child in undirected.Edges[parent.Id])
                {
                    Assert.True(undirected.Edges[child.Child.Id].Any(c => c.Child.Id == parent.Id));
                }
            }

            //make sure we did not add anything redundant
            foreach (var parents in undirected.Nodes.Zip(maybeUndirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());

                var undirectedEdges = undirected.Edges[parents.First.Id].Select(x => x.Child);
                var maybeUndirectedEdges = maybeUndirected.Edges[parents.Second.Id].Select(x => x.Child);

                var diff = maybeUndirectedEdges.Except(undirectedEdges, new NodeEqualityComparer());
                Assert.Empty(diff);

                diff = undirectedEdges.Except(maybeUndirectedEdges, new NodeEqualityComparer());

                foreach (var n in diff)
                {
                    Assert.True(maybeUndirected.Edges[n.Id].Any(x => x.Child.Id == parents.First.Id));
                }
            }
            validateThereIsNoCopiesAndParentInEdges(undirected.Nodes,undirected.Edges);
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
            _GraphStructure.Do
            .ConnectNodes(children_count);
            validateThereIsNoCopiesAndParentInEdges(_GraphStructure.Nodes,_GraphStructure.Edges);
            ensureRightCountOfEdgesPerNode(_GraphStructure.Nodes, 100, 101);
        }
        [Fact]
        public void ConnectRandomlyWorks()
        {
            int minCountOfNodes = Random.Shared.Next(5);
            int maxCountOfNodes = Random.Shared.Next(5)+100;
            _GraphStructure.Do
            .ConnectRandomly(minCountOfNodes, maxCountOfNodes);

            validateThereIsNoCopiesAndParentInEdges(_GraphStructure.Nodes,_GraphStructure.Edges);
            ensureRightCountOfEdgesPerNode(_GraphStructure.Nodes, minCountOfNodes, maxCountOfNodes);
        }
        [Fact]
        public void MeanNodeEdgesCount_Works(){
            _GraphStructure.Do.ConnectRandomly(0,5);
            float expected = (float)(_GraphStructure.Edges.Count)/_GraphStructure.Nodes.Count;
            float actual = _GraphStructure.MeanNodeEdgesCount();
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void Isolate_Works(){
            _GraphStructure.Do.ConnectRandomly(1,5);
            _GraphStructure.Do.Isolate(x=>x.Id%2==0);
            foreach(var n in _GraphStructure.Nodes){
                if(n.Id%2==0)
                    Assert.Empty(_GraphStructure.Edges[n.Id]);
                foreach(var e in _GraphStructure.Edges[n.Id]){
                    Assert.True(e.Child.Id%2!=0);
                }
            }
        }
        [Fact]
        public void CountParents_Works(){
            _GraphStructure.Do.RemoveEdges(x=>true);
            var parentsCount = _GraphStructure.CountParents();
            Assert.All(parentsCount,x=>Assert.Equal(x.Value,0));

            _GraphStructure.Do.ConnectRandomly(2,5);
            parentsCount = _GraphStructure.CountParents();

            foreach(var e in _GraphStructure.Edges){
                parentsCount[e.Child.Id]--;
            }

            Assert.All(parentsCount,x=>Assert.Equal(x.Value,0));
        }
        [Fact]
        public void ReverseEdges_Works(){
            _GraphStructure.Do.ConnectRandomly(1,5);
            var before_reverse = _GraphStructure.Converter.ToConnectionsList();
            _GraphStructure.Do.ReverseEdges();
            var after_reverse = _GraphStructure.Converter.ToConnectionsList();
            _GraphStructure.Do.ReverseEdges();
            var after_two_reverses = _GraphStructure.Converter.ToConnectionsList();
            foreach(var e in before_reverse.Zip(after_two_reverses)){
                Assert.Equal(e.First.Key,e.Second.Key);
                var expected = e.First.Value.ToList();
                var actual = e.Second.Value.ToList();
                expected.Sort();
                actual.Sort();
                Assert.Equal(expected,actual);
            }
            Assert.NotEqual(before_reverse,after_reverse);
        }
        [Fact]
        public void Clone_Works(){
            _GraphStructure.Do.ConnectRandomly(1,5);
            var clone = _GraphStructure.Clone();
            Assert.Equal(_GraphStructure.Nodes,clone.Nodes);
            Assert.Equal(_GraphStructure.Edges.Count,clone.Edges.Count);
            var t1 = _GraphStructure.Converter.ToConnectionsList();
            var t2 = clone.Converter.ToConnectionsList();
            Assert.Equal(t1,t2);
            clone.Do.RemoveEdges(x=>x.Child.Id%2==0);
            t1 = _GraphStructure.Converter.ToConnectionsList();
            t2 = clone.Converter.ToConnectionsList();
            Assert.NotEqual(t1,t2);
        }
        [Fact]
        public void RemoveIsolatedNodes_Works(){
            _GraphStructure.Do.ConnectRandomly(2,6);
            var before = _GraphStructure.Clone();
            before.Do.RemoveNodes(x=>x.Id%2==0);

            _GraphStructure.Do.Isolate(x=>x.Id%2==0).RemoveIsolatedNodes();
            var after = _GraphStructure;
            
            Assert.Equal(before.Nodes,after.Nodes);
            foreach(var n in before.Nodes.Zip(after.Nodes)){
                Assert.Equal(n.First.Id,n.Second.Id);
            }
        }
        public void validateThereIsNoCopiesAndParentInEdges(INodeSource<TestNode> nodes,IEdgeSource<TestEdge> edges)
        {
            foreach (var parent in nodes)
            {
                var parentEdges = edges[parent.Id];
                Assert.Equal(parentEdges.Distinct(), edges[parent.Id]);
                Assert.False(parentEdges.Any(child => child.Child.Id == parent.Id), $"There is parent in children. Parent : {parent.Id}");
            }
        }
        public void ensureRightCountOfEdgesPerNode(IEnumerable<TestNode> nodes, int minEdges, int maxEdges)
        {
            Assert.NotEmpty(nodes);
            foreach (var node in nodes)
            {
                var edges = _GraphStructure.Edges[node.Id];
                var edgesCount = edges.Count();
                Assert.True(edgesCount >= minEdges && edgesCount < maxEdges,$"{edgesCount} >= {minEdges} && {edgesCount} < {maxEdges}");
            }
        }
        public void ensureDirected(IGraphStructure<TestNode,TestEdge> directed,IGraphStructure<TestNode,TestEdge> undirected){

            Assert.Equal(directed.Nodes, undirected.Nodes);

            //make sure each child have no connection to parent
            foreach (var edge in directed.Edges)
            {
                Assert.False(directed.Edges[edge.Child.Id].Any(c => c.Child.Id == edge.Parent.Id));
            }

            //make sure we did not remove anything that is not connected to node
            foreach (var parents in directed.Nodes.Zip(undirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());
                var directedEdges = directed.Edges[parents.First.Id].Select(x => x.Child);
                var undirectedEdges = undirected.Edges[parents.Second.Id].Select(x => x.Child);
                var diff = undirectedEdges.Except(directedEdges, new NodeEqualityComparer());

                foreach (var n in diff.Select(x=>x as TestNode))
                {
                    Assert.True(directed.Edges[n.Id].Any(x => x.Child.Id == parents.First.Id) || undirected.Edges[n.Id].Any(x => x.Child.Id == parents.First.Id));
                }
            }
        }
        /// <summary>
        /// Checks that set of edges is closed on set of nodes
        /// </summary>
        public static void CheckForIntegrity<TNode,TEdge>(INodeSource<TNode> nodes, IEdgeSource<TEdge> edges)
        where TNode : INode
        where TEdge : IEdge
        {
            foreach(var e in edges){
                Assert.True(nodes.TryGetNode(e.Parent.Id,out var _));
                Assert.True(nodes.TryGetNode(e.Child.Id,out var _));
            }
        }

    }
}