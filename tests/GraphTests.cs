
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;
using Xunit;
namespace tests
{
    public class GraphTests
    {
        [Fact]
        public void AddVesitor_ThrowsIfOutOfRange(){
            var graph = new Graph(Enumerable.Range(1,5).Select(i=>new Node(i)));
            var vesitor = new ActionVesitor(node=>{});
            Assert.Throws<IndexOutOfRangeException>(()=> graph.AddVesitor(vesitor,22));
        }
        [Fact]
        public void RemoveVesitor_Works(){
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            var graph = new Graph(nodes);
            
            var childs1 = new List<NodeBase>();
            var childs2 = new List<NodeBase>();

            
            var vesitor1 = new ActionVesitor(node =>{
                lock(childs1)
                    childs1.Add(node);
            });
            var vesitor2 = new ActionVesitor(node =>{
                lock(childs2)
                    childs2.Add(node);
            });
            graph.AddVesitor(vesitor1,1);
            graph.AddVesitor(vesitor2,2);

            graph.Start();
            childs1.Clear();
            childs2.Clear();
            
            graph.Step();
            childs1.Sort((v1, v2) => v1.Id - v2.Id);
            nodes[1].Childs.Sort((v1,v2)=>v1.Id-v2.Id);

            Assert.Equal(childs1,nodes[1].Childs);
            Assert.Equal(childs2,nodes[2].Childs);

            childs1.Clear();
            childs2.Clear();

            graph.RemoveVesitor(vesitor1);
            Assert.Throws<KeyNotFoundException>(()=>graph.Step(vesitor1));

            childs1.Clear();
            childs2.Clear();

            graph.Step();
            Assert.Equal(childs1.Count,0);
            Assert.NotEqual(childs2.Count,0);

        }
        [Fact]
        public void Start_Step_WrongVesitorThrows(){
            var graph = new Graph(new List<Node>());
            var vesitor1 = new ActionVesitor(node =>{});
            var vesitor2 = new ActionVesitor(node =>{});
            
            Assert.Throws<InvalidOperationException>(()=>
                graph.AddVesitor(vesitor1));
            
            Assert.Throws<InvalidOperationException>(()=>
                graph.AddVesitor(vesitor1,0));

            Assert.Throws<KeyNotFoundException>(()=>
                graph.Start(vesitor2));
        }
        [Fact]
        public void Graph_Vesit_ValidateOrder()
        {
            IEnumerable<Node> nodes = null;
            IGraph graph = null;
            nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            graph = new Graph(nodes);

            validate_graphOrder(graph, nodes, new Random().Next(nodes.Count()));

        }
  
        [Fact]
        public void Graph_Vesit_ValidateOrderMultipleVesitors()
        {
            const int index1 = 3;
            const int index2 = 9;

            IEnumerable<Node> nodes = null;
            IGraph graph;
            
            nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            graph = new Graph(nodes);
            validate_graphOrderMultipleVesitors(graph,index1,index2);
        }

        public static void validate_graphOrder(IGraph graph, IEnumerable<NodeBase> nodes, int index)
        {

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

            graph.AddVesitor(vesitor, index);
            graph.Start();

            buf_gen = next_gen.ToList();
            buf_gen.Sort((v1,v2)=>v1.Id-v2.Id);

            next_gen.Clear();
            current_gen.Clear();

            for (int i = 0; i < 50; i++)
            {
                graph.Step();
                current_gen.Sort((v1,v2)=>v1.Id-v2.Id);
                Assert.Equal(buf_gen.Count,current_gen.Count);
                Assert.Equal(buf_gen, current_gen);

                buf_gen = next_gen.ToList();
                buf_gen.Sort((v1,v2)=>v1.Id-v2.Id);

                next_gen.Clear();
                current_gen.Clear();
            }
        }
        public static void validate_graphOrderMultipleVesitors(IGraph graph, int index1,int index2){
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

            graph.AddVesitor(vesitor1, index1);
            graph.AddVesitor(vesitor2, index2);

            graph.Start();
            //vesitor 1
            buf_gen1 = next_gen1.ToList();
            buf_gen1.Sort((v1,v2)=>v1.Id-v2.Id);

            next_gen1.Clear();
            current_gen1.Clear();
            //vesitor 2
            buf_gen2 = next_gen2.ToList();
            buf_gen2.Sort((v1,v2)=>v1.Id-v2.Id);

            next_gen2.Clear();
            current_gen2.Clear();

            for (int i = 0; i < 100; i++)
            {
                if (i % 2 == 0)
                {
                    graph.Step();
                    check1();
                    check2();
                }

                if (i % 3 == 0)
                {
                    graph.Step(vesitor1);
                    check1();
                }

                if (i % 5 == 0)
                {
                    graph.Step(vesitor2);
                    check2();
                }

            }
            void check1()
            {
                current_gen1.Sort((v1,v2)=>v1.Id-v2.Id);
                Assert.Equal(buf_gen1.Count, current_gen1.Count);
                Assert.Equal(buf_gen1, current_gen1);
                buf_gen1 = next_gen1.ToList();
                buf_gen1.Sort((v1,v2)=>v1.Id-v2.Id);
                next_gen1.Clear();
                current_gen1.Clear();
            }
            void check2()
            {
                current_gen2.Sort((v1,v2)=>v1.Id-v2.Id);
                Assert.Equal(buf_gen2.Count, current_gen2.Count);
                buf_gen2 = next_gen2.ToList();
                buf_gen2.Sort((v1,v2)=>v1.Id-v2.Id);
                next_gen2.Clear();
                current_gen2.Clear();
            }
        }
        
    }
}
