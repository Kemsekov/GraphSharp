
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using Xunit;
namespace tests
{
    public class GraphTests
    {
        [Fact]
        public void Graph_VisitAll(){
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            var expected_value = new List<NodeBase>();
            var visitor = new ActionVisitor((n,v)=>{
                lock(expected_value)
                expected_value.Add(n);
            });
            var graph = new Graph(nodes);
            graph.AddVisitor(visitor,1);

            graph.Step();
            expected_value.Clear();
            graph.Step();

            expected_value.Sort();
            nodes[1].Childs.Sort();
            Assert.Equal(expected_value,nodes[1].Childs);
            expected_value.Clear();
            graph.Step();
            expected_value.Sort();
            var childs = nodes[1].Childs.SelectMany(n=>n.Childs).ToList();
            childs.Sort();
            Assert.Equal(expected_value,childs);
        }
        [Fact]
        public void Graph_ValidateOrderAgain()
        {
            var nodes = new Node[10];
            var expectedNodes1 = new int[][]{
                new int[] {0,7},
                new int[] {1,4,9},
                new int[] {2,3,5,8},
                new int[] {0,1,3,6,7,9},
                new int[] {1,2,3,4,7,8,9}
            };

            var expectedNodes2 = new int[][]{
                new int[] {1,5},
                new int[] {0,1,2,3,6,8},
                new int[] {1,2,3,4,7,8,9},
                new int[] {1,2,3,5,7,8,9},
                new int[] {0,1,2,3,6,7,8,9}
            };

            for (int i = 0; i < nodes.Count(); i++)
            {
                nodes[i] = new Node(i);
            }
            //init graph with nodes
            {
                nodes[0].AddChild(nodes[4]);
                nodes[0].AddChild(nodes[1]);

                nodes[1].AddChild(nodes[2]);
                nodes[1].AddChild(nodes[3]);
                nodes[1].AddChild(nodes[8]);

                nodes[2].AddChild(nodes[7]);
                nodes[2].AddChild(nodes[9]);

                nodes[3].AddChild(nodes[7]);

                nodes[4].AddChild(nodes[5]);

                nodes[5].AddChild(nodes[0]);
                nodes[5].AddChild(nodes[1]);
                nodes[5].AddChild(nodes[3]);
                nodes[5].AddChild(nodes[6]);

                nodes[6].AddChild(nodes[3]);

                nodes[7].AddChild(nodes[1]);
                nodes[7].AddChild(nodes[9]);

                nodes[8].AddChild(nodes[9]);

                nodes[9].AddChild(nodes[3]);
            }

            var graph = new Graph(nodes);
            var visitor1_store = new List<NodeBase>();
            var visitor2_store = new List<NodeBase>();

            ActionVisitor visitor1;
            ActionVisitor visitor2;

            for (int i = 0; i < 20; i++)
            {
                visitor1 = new ActionVisitor((node,visited) =>
                {
                    if(!visited)
                    lock (visitor1_store) visitor1_store.Add(node);
                }, null,null);

                visitor2 = new ActionVisitor((node,visited) =>
                {
                    if(!visited)
                    lock (visitor2_store) visitor2_store.Add(node);
                },null,null);
                graph.Clear();

                graph.AddVisitor(visitor1, 0, 7);
                graph.AddVisitor(visitor2, 5, 1);

                foreach (var ex1 in expectedNodes1)
                {
                    visitor1_store.Clear();
                    graph.Step(visitor1);
                    visitor1_store.Sort();
                    Assert.Equal(visitor1_store.Select(v => v.Id), ex1);
                }

                foreach (var ex2 in expectedNodes2)
                {
                    visitor2_store.Clear();
                    graph.Step(visitor2);
                    visitor2_store.Sort();
                    Assert.Equal(visitor2_store.Select(v => v.Id), ex2);
                }

                graph.Clear();

                graph.AddVisitor(visitor1, 0, 7);
                graph.AddVisitor(visitor2, 5, 1);

                foreach (var ex in expectedNodes1.Zip(expectedNodes2))
                {
                    visitor1_store.Clear();
                    visitor2_store.Clear();
                    graph.Step();
                    visitor1_store.Sort();
                    visitor2_store.Sort();
                    Assert.Equal(visitor1_store.Select(v => v.Id), ex.First);
                    Assert.Equal(visitor2_store.Select(v => v.Id), ex.Second);
                }
            }

        }
        [Fact]
        public void ForthBackwardVisitors_Test()
        {
            for (int k = 0; k < 20; k++)
            {
                var nodes = new Node[14];
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i] = new Node(i);
                }
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (i + 1 < nodes.Length)
                    {
                        nodes[i].AddChild(nodes[i + 1]);
                    }
                }

                for (int i = nodes.Length - 1; i > 0; i--)
                {
                    if (i - 1 >= 0)
                    {
                        nodes[i].AddChild(nodes[i - 1]);
                    }
                }

                var forward_list = new List<NodeBase>();
                var back_list = new List<NodeBase>();

                var forward_visitor = new ActionVisitor((node,visited) =>
                {
                    if(!visited)
                    lock (forward_list)
                        forward_list.Add(node);
                },
                null,
                //select happening before visit
                node =>
                {
                    if (forward_list.Count == 0) return true;
                    return forward_list.Last().Id < node.Id;
                });

                var back_visitor = new ActionVisitor(
                    (node,visited) =>
                    {
                        if(!visited)
                        lock (back_list)
                            back_list.Add(node);
                    },
                    null,
                    node =>
                    {
                        if (back_list.Count == 0) return true;
                        return back_list.Last().Id > node.Id;
                    });

                var graph = new Graph(nodes);

                graph.AddVisitor(forward_visitor, 0);
                graph.AddVisitor(back_visitor, 13);

                graph.Step();
                for (int i = 0; i < nodes.Length; i++)
                    graph.Step();
                back_list.Reverse();
                Assert.Equal(forward_list, back_list);
            }
        }
        [Fact]
        public void VisitorSelect_Works()
        {
            IEnumerable<Node> nodes = null;
            IGraph graph = null;
            nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            graph = new Graph(nodes);
            validate_graphOrder(graph, nodes, 3, node => node.Id % 2 == 0);

        }
        [Fact]
        public void AddVisitor_ThrowsIfOutOfRange()
        {
            var graph = new Graph(Enumerable.Range(1, 5).Select(i => new Node(i)));
            var visitor = new ActionVisitor((node,visited) => { });
            Assert.Throws<IndexOutOfRangeException>(() => graph.AddVisitor(visitor, 22));
        }
        [Fact]
        public void RemoveVisitor_Works()
        {
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            var graph = new Graph(nodes);

            var childs1 = new List<NodeBase>();
            var childs2 = new List<NodeBase>();


            var visitor1 = new ActionVisitor((node,visited) =>
            {
                if(!visited)
                lock (childs1)
                    childs1.Add(node);
            });
            var visitor2 = new ActionVisitor((node,visited) =>
            {
                if(!visited)
                lock (childs2)
                    childs2.Add(node);
            });
            graph.AddVisitor(visitor1, 1);
            graph.AddVisitor(visitor2, 2);

            graph.Step();
            childs1.Clear();
            childs2.Clear();

            graph.Step();
            childs1.Sort((v1, v2) => v1.Id - v2.Id);
            nodes[1].Childs.Sort((v1, v2) => v1.Id - v2.Id);
            nodes[2].Childs.Sort((v1, v2) => v1.Id - v2.Id);

            Assert.Equal(childs1, nodes[1].Childs);
            Assert.Equal(childs2.Count, nodes[2].Childs.Count);
            Assert.Equal(childs2, nodes[2].Childs);

            childs1.Clear();
            childs2.Clear();

            graph.RemoveVisitor(visitor1);
            Assert.Throws<KeyNotFoundException>(() => graph.Step(visitor1));
            childs1.Clear();
            childs2.Clear();

            graph.Step();
            Assert.Equal(childs1.Count, 0);
            var __nodes = graph.GetType().GetProperty("_nodes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(graph) as NodeBase[];

            Assert.NotEqual(childs2.Count, 0);

        }
        [Fact]
        public void Step_WrongVisitorThrowsOutOfRangeTrows()
        {

            var graph = new Graph(new List<Node>() { new Node(0), new Node(1), new Node(2), new Node(3) });
            var visitor1 = new ActionVisitor((node,visited) => { });
            var visitor2 = new ActionVisitor((node,visited) => { });

            graph.AddVisitor(visitor1, 1);

            Assert.Throws<IndexOutOfRangeException>(() =>
                graph.AddVisitor(visitor1, 10));

            Assert.Throws<KeyNotFoundException>(() =>
                graph.Step(visitor2));
        }
        [Fact]
        public void Graph_Visit_ValidateOrder()
        {
            IEnumerable<Node> nodes = null;
            IGraph graph = null;
            nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            graph = new Graph(nodes);

            validate_graphOrder(graph, nodes, new Random().Next(nodes.Count()));

        }
        [Fact]
        public void Graph_Visit_ValidateOrderMultipleVisitors()
        {
            const int index1 = 3;
            const int index2 = 9;

            IEnumerable<Node> nodes = null;
            IGraph graph;

            nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(1000, 30, 70);
            graph = new Graph(nodes);
            validate_graphOrderMultipleVisitors(graph, index1, index2);
        }
        public static void validate_graphOrder(IGraph graph, IEnumerable<NodeBase> nodes, int index, Func<NodeBase, bool> selector = null)
        {

            var next_gen = new HashSet<NodeBase>();
            var current_gen = new List<NodeBase>();
            var buf_gen = new List<NodeBase>();

            var visitor = new ActionVisitor((node,visited) =>
            {
                if(!visited)
                lock (nodes)
                {
                    current_gen.Add(node);
                    node.Childs.ForEach(n =>
                    {
                        if (selector is null)
                            next_gen.Add(n);
                        else if (selector(n))
                            next_gen.Add(n);
                    });
                }
            }, null, selector);

            graph.AddVisitor(visitor, index);
            graph.Step();

            buf_gen = next_gen.ToList();
            buf_gen.Sort((v1, v2) => v1.Id - v2.Id);

            next_gen.Clear();
            current_gen.Clear();

            for (int i = 0; i < 50; i++)
            {
                graph.Step();
                current_gen.Sort((v1, v2) => v1.Id - v2.Id);
                Assert.Equal(buf_gen.Count, current_gen.Count);
                Assert.Equal(buf_gen, current_gen);

                buf_gen = next_gen.ToList();
                buf_gen.Sort((v1, v2) => v1.Id - v2.Id);

                next_gen.Clear();
                current_gen.Clear();
            }
        }
        public static void validate_graphOrderMultipleVisitors(IGraph graph, int index1, int index2, Func<NodeBase, bool> selector = null)
        {
            var next_gen1 = new HashSet<NodeBase>();
            var current_gen1 = new List<NodeBase>();
            var buf_gen1 = new List<NodeBase>();

            var next_gen2 = new HashSet<NodeBase>();
            var current_gen2 = new List<NodeBase>();
            var buf_gen2 = new List<NodeBase>();

            var visitor1 = new ActionVisitor((node,visited) =>
            {
                if(!visited)
                lock (next_gen1)
                {
                    current_gen1.Add(node);
                    node.Childs.ForEach(n =>
                    {
                        if (selector is null)
                            next_gen1.Add(n);
                        else if (selector(n))
                            next_gen1.Add(n);
                    });
                }
            });

            var visitor2 = new ActionVisitor((node,visited) =>
            {
                if(!visited)
                lock (next_gen2)
                {
                    current_gen2.Add(node);
                    node.Childs.ForEach(n =>
                    {
                        if (selector is null)
                            next_gen2.Add(n);
                        else if (selector(n))
                            next_gen2.Add(n);
                    });
                }
            });

            graph.AddVisitor(visitor1, index1);
            graph.AddVisitor(visitor2, index2);

            graph.Step();
            //visitor 1
            buf_gen1 = next_gen1.ToList();
            buf_gen1.Sort((v1, v2) => v1.Id - v2.Id);

            next_gen1.Clear();
            current_gen1.Clear();
            //visitor 2
            buf_gen2 = next_gen2.ToList();
            buf_gen2.Sort((v1, v2) => v1.Id - v2.Id);

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
                    graph.Step(visitor1);
                    check1();
                }

                if (i % 5 == 0)
                {
                    graph.Step(visitor2);
                    check2();
                }

            }
            void check1()
            {
                current_gen1.Sort((v1, v2) => v1.Id - v2.Id);
                Assert.Equal(buf_gen1.Count, current_gen1.Count);
                Assert.Equal(buf_gen1, current_gen1);
                buf_gen1 = next_gen1.ToList();
                buf_gen1.Sort((v1, v2) => v1.Id - v2.Id);
                next_gen1.Clear();
                current_gen1.Clear();
            }
            void check2()
            {
                current_gen2.Sort((v1, v2) => v1.Id - v2.Id);
                Assert.Equal(buf_gen2.Count, current_gen2.Count);
                buf_gen2 = next_gen2.ToList();
                buf_gen2.Sort((v1, v2) => v1.Id - v2.Id);
                next_gen2.Clear();
                current_gen2.Clear();
            }
        }

    }
}
