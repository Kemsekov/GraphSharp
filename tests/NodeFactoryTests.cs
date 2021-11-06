using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Nodes;
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
        public void NodeGraphFactory_EnsureNodesCount()
        {
            Assert.Equal(_nodes.Count, _nodes_count);
        }
        [Fact]
        public void NodeGraphFactory_ValidateConnected()
        {
            int children_count = 100;
            NodeGraphFactory.ConnectNodes(_nodes, children_count);
            validateThereIsNoCopiesAndParentInChildren(_nodes);
            Parallel.ForEach(_nodes,node=>
            {
                Assert.Equal(node.Children.Count, children_count);
                validateThereIsNoCopiesAndParentInChildren(node.Children.Select(child => child.Node).ToList());
            });
        }
        [Fact]
        public void NodeGraphFactory_ValidateRandomConnected(){
            const int min_count_of_nodes = 5;
            const int max_count_of_nodes = 30;
            NodeGraphFactory.ConnectRandomCountOfNodes(_nodes,min_count_of_nodes,max_count_of_nodes);
            validateThereIsNoCopiesAndParentInChildren(_nodes);
            Parallel.ForEach(_nodes,node=>
            {
                Assert.True(node.Children.Count is >= min_count_of_nodes and <= max_count_of_nodes);
                validateThereIsNoCopiesAndParentInChildren(node.Children.Select(child=>child.Node).ToList());
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