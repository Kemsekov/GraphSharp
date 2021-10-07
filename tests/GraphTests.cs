
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using Xunit;
namespace tests
{
    public class GraphTests
    {
        [Fact]
        public void Vesit_ValidateOrder()
        {
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            const int index = 3;

            var next_gen = new HashSet<NodeBase>();
            var current_gen = new List<NodeBase>();
            var buf_gen = new List<NodeBase>();

            var vesitor = new ActionVesitor(node =>
            {
                lock (nodes)
                {
                    current_gen.Add(node);
                    node.Childs.ForEach(n => next_gen.Add(n));
                }
            });

            var graph = new Graph(nodes);
            graph.AddVesitor(vesitor, index);
            graph.Start();
            //current_get here is node3
            //next_gen is what will be current_gen in next iteration

            //save next_gen
            buf_gen = next_gen.ToList();
            buf_gen.Sort((v1, v2) => v1.Id - v2.Id);

            next_gen.Clear();
            current_gen.Clear();

            for (int i = 0; i < 50; i++)
            {
                graph.Step();
                current_gen.Sort((v1, v2) => v1.Id - v2.Id);
                Assert.Equal(buf_gen, current_gen);

                buf_gen = next_gen.ToList();
                buf_gen.Sort((v1, v2) => v1.Id - v2.Id);

                next_gen.Clear();
                current_gen.Clear();
            }
        }
        [Fact]
        public void Vesit_ValidateOrderMultipleVesitors()
        {
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            const int index1 = 3;
            const int index2 = 9;

            var next_gen1 = new HashSet<NodeBase>();
            var current_gen1 = new List<NodeBase>();
            var buf_gen1 = new List<NodeBase>();

            var next_gen2 = new HashSet<NodeBase>();
            var current_gen2 = new List<NodeBase>();
            var buf_gen2 = new List<NodeBase>();

            var vesitor1 = new ActionVesitor(node =>
            {
                lock (next_gen1)
                {
                    current_gen1.Add(node);
                    node.Childs.ForEach(n => next_gen1.Add(n));
                }
            });

            var vesitor2 = new ActionVesitor(node =>
            {
                lock (next_gen2)
                {
                    current_gen2.Add(node);
                    node.Childs.ForEach(n => next_gen2.Add(n));
                }
            });

            var graph = new Graph(nodes);

            graph.AddVesitor(vesitor1, index1);
            graph.AddVesitor(vesitor2, index2);

            graph.Start();
            //vesitor 1
            buf_gen1 = next_gen1.ToList();
            buf_gen1.Sort((v1, v2) => v1.Id - v2.Id);

            next_gen1.Clear();
            current_gen1.Clear();
            //vesitor 2
            buf_gen2 = next_gen2.ToList();
            buf_gen2.Sort((v1, v2) => v1.Id - v2.Id);

            next_gen2.Clear();
            current_gen2.Clear();

            for (int i = 0; i < 100; i++)
            {
                if(i%2==0){
                    graph.Step();
                    check1();
                    check2();
                }

                if(i%3==0){
                    graph.Step(vesitor1);
                    check1();
                }
                
                if(i%5==0){
                    graph.Step(vesitor2);
                    check2();
                }

            }
            void check1(){
                current_gen1.Sort((v1, v2) => v1.Id - v2.Id);
                Assert.Equal(buf_gen1, current_gen1);
                buf_gen1 = next_gen1.ToList();
                buf_gen1.Sort((v1, v2) => v1.Id - v2.Id);
                next_gen1.Clear();
                current_gen1.Clear();
            }
            void check2(){
                current_gen2.Sort((v1, v2) => v1.Id - v2.Id);
                Assert.Equal(buf_gen2, current_gen2);
                buf_gen2 = next_gen2.ToList();
                buf_gen2.Sort((v1, v2) => v1.Id - v2.Id);
                next_gen2.Clear();
                current_gen2.Clear();
            }
        }
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
                Assert.Equal(node.Childs.Count,childs_count);
                
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
