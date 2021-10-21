using System.Collections.Generic;
using System.Linq;
using GraphSharp;
using GraphSharp.Nodes;
using Xunit;

namespace tests
{
    public class NodeFactoryTests_Generic
    {
        [Fact]
        public void NodeGraphFactory_CreateConnectedParallel_Validate(){
            const int Children_count = 100;
            const int nodes_count = 5000;
            var nodes = NodeGraphFactory.CreateConnectedParallel<Node<object>,object>(nodes_count,Children_count);
            validateConnected(nodes.Select(n=>n as NodeBase<object>).ToList(),nodes_count,Children_count);
        }
        [Fact]
        public void NodeGraphFactory_CreateRandomConnectedParallel_Validate(){
            const int max_Children_count = 100;
            const int min_Children_count = 10;
            const int nodes_count = 5000;
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>,object>(nodes_count,max_Children_count, min_Children_count);
            validateRandomConnected(nodes.Select(n=>n as NodeBase<object>).ToList(),nodes_count,max_Children_count,min_Children_count);
        }

        [Fact]
        public void NodeGraphFactory_CreateRandomConnected(){
            const int max_Children_count = 100;
            const int min_Children_count = 10;
            const int nodes_count = 5000;
            var nodes = NodeGraphFactory.CreateRandomConnected<Node<object>,object>(nodes_count,max_Children_count, min_Children_count);
            validateRandomConnected(nodes.Select(n=>n as NodeBase<object>).ToList(),nodes_count,max_Children_count,min_Children_count);
        }
        [Fact]
        public void NodeGraphFactory_CreateConnected_Validate(){
            const int Children_count = 100;
            const int nodes_count = 5000;
            var nodes = NodeGraphFactory.CreateConnected<Node<object>,object>(nodes_count,Children_count);
            validateConnected(nodes.Select(n=>n as NodeBase<object>).ToList(),nodes_count,Children_count);
        }
        private void validateRandomConnected<T>(IList<NodeBase<T>> nodes,int nodes_count,int max_Children_count, int min_Children_count){
            Assert.Equal(nodes.Count,nodes_count);
            foreach(var node in nodes){
                //check if Children count of node equal to Children_count
                Assert.True(node.Children.Count>=min_Children_count,$"min is {min_Children_count}, but Children count is {node.Children.Count}");
                Assert.True(node.Children.Count<=max_Children_count,$"max is {max_Children_count}, but Children count is {node.Children.Count}");
                
                //check if Children of node does not contains itself
                foreach(var child in node.Children)
                    Assert.NotEqual(child.NodeBase,node);
                
                //check if Children has no copies
                var Children =new List<NodeBase<T>>(node.Children.Select(n=>n.NodeBase));
                var hash_set = new HashSet<NodeBase<T>>(Children);
                Children.Sort((v1,v2)=>v1.Id-v2.Id);
                var hash_set_Children = hash_set.ToList();
                hash_set_Children.Sort((v1,v2)=>v1.Id-v2.Id);
                Assert.Equal(Children,hash_set_Children);

            }
        }
        private void validateConnected<T>(IList<NodeBase<T>> nodes,int nodes_count,int Children_count){
            Assert.Equal(nodes.Count,nodes_count);
            foreach(var node in nodes){
                //check if Children count of node equal to Children_count
                Assert.True(node.Children.Count<=Children_count);
                Assert.True(node.Children.Count>=(Children_count-1));
                //check if Children of node does not contains itself
                foreach(var child in node.Children)
                    Assert.NotEqual(child.NodeBase,node);
                
                //check if Children has no copies
                var Children =new List<NodeBase<T>>(node.Children.Select(n=>n.NodeBase));
                var hash_set = new HashSet<NodeBase<T>>(Children);
                Children.Sort((v1,v2)=>v1.Id-v2.Id);
                var hash_set_Children = hash_set.ToList();
                hash_set_Children.Sort((v1,v2)=>v1.Id-v2.Id);
                Assert.Equal(Children,hash_set_Children);

            }
        }
   
    }
}