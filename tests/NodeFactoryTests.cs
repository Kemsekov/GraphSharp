using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Children;
using GraphSharp.Nodes;
using tests.Helpers;
using Xunit;

namespace tests
{
    public class NodeFactoryTests
    {

        private int _nodes_count;
        private NodesFactory _nodesFactory;

        public NodeFactoryTests()
        {
            this._nodes_count = 500;
            this._nodesFactory = new NodesFactory().CreateNodes(_nodes_count);
        }
        [Fact]
        public void EnsureMakeDirectedWorks()
        {
            //create two identical nodes list
            var seed = new Random().Next();
            var directed = 
                new NodesFactory(rand : new Random(seed))
                    .CreateNodes(2000)
                    .ConnectNodes(20)
                    .MakeDirected();
            var undirected = 
                new NodesFactory(rand : new Random(seed))
                    .CreateNodes(2000)
                    .ConnectNodes(20);

            Assert.Equal(directed.Nodes, undirected.Nodes);

            //make sure each child have no connection to parent
            foreach (var parent in directed.Nodes)
            {
                foreach (var child in parent.Children)
                {
                    Assert.False(child.Node.Children.Any(c => c.Node.Id == parent.Id));
                }
            }

            //make sure we did not remove anything that is not connected to node
            foreach (var parents in directed.Nodes.Zip(undirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());
                var directedChildren = parents.First.Children.Select(x => x.Node);
                var undirectedChildren = parents.Second.Children.Select(x => x.Node);
                var diff = undirectedChildren.Except(directedChildren, new NodeEqualityComparer());
                
                foreach (var n in diff)
                {
                    Assert.True(n.Children.Any(x => x.Node.Id == parents.First.Id));
                }
            }

        }
        [Fact]
        public void EnsureMakeUndirectedWorks()
        {
            var seed = new Random().Next();
            var maybeUndirected = 
                new NodesFactory(rand : new Random(seed))
                .CreateNodes(2000)
                .ConnectNodes(20);
            var undirected = 
                new NodesFactory(rand : new Random(seed))
                .CreateNodes(2000)
                .ConnectNodes(20)
            //one of them make 100% undirected
                .MakeUndirected();

            //ensure they are the same
            Assert.Equal(maybeUndirected.Nodes, undirected.Nodes);

            //make sure each child have connection to parent
            foreach (var parent in undirected.Nodes)
            {
                foreach (var child in parent.Children)
                {
                    Assert.True(child.Node.Children.Any(c => c.Node.Id == parent.Id));
                }
            }

            //make sure we did not add anything redundant
            foreach (var parents in undirected.Nodes.Zip(maybeUndirected.Nodes))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());
                
                var undirectedChildren = parents.First.Children.Select(x => x.Node);
                var maybeUndirectedChildren = parents.Second.Children.Select(x => x.Node);
                
                var diff = maybeUndirectedChildren.Except(undirectedChildren, new NodeEqualityComparer());
                Assert.Empty(diff);
                
                diff = undirectedChildren.Except(maybeUndirectedChildren, new NodeEqualityComparer());

                foreach (var n in diff)
                {
                    Assert.True(maybeUndirected.Nodes[n.Id].Children.Any(x=>x.Node.Id==parents.First.Id));
                }
            }


        }
        [Fact]
        public void EnsureNodesCount()
        {
            Assert.Equal(_nodesFactory.Nodes.Count, _nodes_count);
        }
        [Fact]
        public void ValidateConnected()
        {
            int children_count = 100;
            _nodesFactory.ConnectNodes(children_count);
            validateThereIsNoCopiesAndParentInChildren(_nodesFactory.Nodes);
            Parallel.ForEach(_nodesFactory.Nodes, node =>
             {
                 Assert.Equal(node.Children.Count, children_count);
                 validateThereIsNoCopiesAndParentInChildren(node.Children.Select(child => child.Node).ToList());
             });
        }
        [Fact]
        public void ValidateRandomConnected()
        {
            const int min_count_of_nodes = 5;
            const int max_count_of_nodes = 30;
            _nodesFactory.ConnectRandomly(min_count_of_nodes,max_count_of_nodes);
            validateThereIsNoCopiesAndParentInChildren(_nodesFactory.Nodes);
            Parallel.ForEach(_nodesFactory.Nodes, node =>
             {
                 Assert.True(node.Children.Count is >= min_count_of_nodes and <= max_count_of_nodes);
                 validateThereIsNoCopiesAndParentInChildren(node.Children.Select(child => child.Node).ToList());
             });
        }
        public void validateThereIsNoCopiesAndParentInChildren(IList<INode> nodes)
        {
            foreach (var parent in nodes)
            {
                Assert.Equal(parent.Children.Distinct(), parent.Children);
                Assert.False(parent.Children.Any(child => child.Node.CompareTo(parent) == 0), $"There is parent in children. Parent : {parent}");
            }
        }

    }
}