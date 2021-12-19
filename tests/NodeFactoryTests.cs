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
        private IList<INode> _nodes;

        public NodeFactoryTests()
        {
            this._nodes_count = 500;
            this._nodes = NodeGraphFactory.CreateNodes(_nodes_count);
        }
        [Fact]
        public void EnsureMakeDirectedWorks()
        {
            //create two identical nodes list

            var directed = NodeGraphFactory.CreateNodes(2000);
            var undirected = NodeGraphFactory.CreateNodes(2000);

            //connect them the same way
            NodeGraphFactory.ConnectNodes(directed, 20,new Random(123));
            NodeGraphFactory.ConnectNodes(undirected, 20,new Random(123));

            //one of them make directed
            NodeGraphFactory.MakeDirected(directed);

            //ensure they are the same
            Assert.Equal(directed, undirected);

            //make sure each child have no connection to parent
            foreach (var parent in directed)
            {
                foreach (var child in parent.Children)
                {
                    Assert.False(child.Node.Children.Any(c=>c.Node.Id==parent.Id));
                }
            }

            //make sure we did not remove anything that is not connected to node
            foreach (var parents in directed.Zip(undirected))
            {
                //ensure they are the facto different objects in memory
                Assert.False(parents.First.GetHashCode() == parents.Second.GetHashCode());
                var directedChildren = parents.First.Children.Select(x=>x.Node);
                var undirectedChildren = parents.Second.Children.Select(x=>x.Node);
                var diff = undirectedChildren.Except(directedChildren,new NodeEqualityComparer());
                foreach(var n in diff){
                    Assert.True(n.Children.Any(x=>x.Node.Id==parents.First.Id));
                }
            }

        }
        [Fact]
        public void EnsureMakeUndirectedWorks()
        {
            var nodes = NodeGraphFactory.CreateNodes(2000);
            var nodes_copy = nodes.ToList();

            NodeGraphFactory.ConnectNodes(nodes, 20);

        }
        [Fact]
        public void EnsureNodesCount()
        {
            Assert.Equal(_nodes.Count, _nodes_count);
        }
        [Fact]
        public void ValidateConnected()
        {
            int children_count = 100;
            NodeGraphFactory.ConnectNodes(_nodes, children_count);
            validateThereIsNoCopiesAndParentInChildren(_nodes);
            Parallel.ForEach(_nodes, node =>
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
            NodeGraphFactory.ConnectRandomCountOfNodes(_nodes, min_count_of_nodes, max_count_of_nodes);
            validateThereIsNoCopiesAndParentInChildren(_nodes);
            Parallel.ForEach(_nodes, node =>
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