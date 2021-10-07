using System.Collections.Generic;
using System.Linq;
using GraphSharp;
using GraphSharp.Nodes;
using Xunit;

namespace tests
{
    public class NodeFactoryTests
    {
        [Fact]
        public void NodeGraphFactory_CreateConnectedParallel_Validate(){
            const int childs_count = 100;
            const int nodes_count = 5000;
            var nodes = NodeGraphFactory.CreateConnectedParallel<Node>(nodes_count,childs_count);
            validateConnected(nodes.Select(n=>n as NodeBase).ToList(),nodes_count,childs_count);
        }
        [Fact]
        public void NodeGraphFactory_CreateRandomConnectedParallel_Validate(){
            const int max_childs_count = 100;
            const int min_childs_count = 10;
            const int nodes_count = 5000;
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(nodes_count,max_childs_count, min_childs_count);
            validateRandomConnected(nodes.Select(n=>n as NodeBase).ToList(),nodes_count,max_childs_count,min_childs_count);
        }

        [Fact]
        public void NodeGraphFactory_CreateRandomConnected(){
            const int max_childs_count = 100;
            const int min_childs_count = 10;
            const int nodes_count = 5000;
            var nodes = NodeGraphFactory.CreateRandomConnected<Node>(nodes_count,max_childs_count, min_childs_count);
            validateRandomConnected(nodes.Select(n=>n as NodeBase).ToList(),nodes_count,max_childs_count,min_childs_count);
        }
        [Fact]
        public void NodeGraphFactory_CreateConnected_Validate(){
            const int childs_count = 100;
            const int nodes_count = 5000;
            var nodes = NodeGraphFactory.CreateConnected<Node>(nodes_count,childs_count);
            validateConnected(nodes.Select(n=>n as NodeBase).ToList(),nodes_count,childs_count);
        }
        private void validateRandomConnected(IList<NodeBase> nodes,int nodes_count,int max_childs_count, int min_childs_count){
            Assert.Equal(nodes.Count,nodes_count);
            foreach(var node in nodes){
                //check if childs count of node equal to childs_count
                Assert.True(node.Childs.Count>=min_childs_count && node.Childs.Count<=max_childs_count);
                
                //check if childs of node does not contains itself
                foreach(var child in node.Childs)
                    Assert.NotEqual(child,node);
                
                //check if childs has no copies
                var childs =new List<NodeBase>(node.Childs);
                var hash_set = new HashSet<NodeBase>(childs);
                childs.Sort((v1,v2)=>v1.Id-v2.Id);
                var hash_set_childs = hash_set.ToList();
                hash_set_childs.Sort((v1,v2)=>v1.Id-v2.Id);
                Assert.Equal(childs,hash_set_childs);

            }
        }
        private void validateConnected(IList<NodeBase> nodes,int nodes_count,int childs_count){
            Assert.Equal(nodes.Count,nodes_count);
            foreach(var node in nodes){
                //check if childs count of node equal to childs_count
                Assert.True(node.Childs.Count<=childs_count && node.Childs.Count>=(childs_count-1));
                
                //check if childs of node does not contains itself
                foreach(var child in node.Childs)
                    Assert.NotEqual(child,node);
                
                //check if childs has no copies
                var childs =new List<NodeBase>(node.Childs);
                var hash_set = new HashSet<NodeBase>(childs);
                childs.Sort((v1,v2)=>v1.Id-v2.Id);
                var hash_set_childs = hash_set.ToList();
                hash_set_childs.Sort((v1,v2)=>v1.Id-v2.Id);
                Assert.Equal(childs,hash_set_childs);

            }
        }
   
    }
}