
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
    public class GraphTests_Generic
    {

        [Fact]
        public void Graph_CheckIfVisitAll(){
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>, object>(1000, 30, 10);
            var graph = new Graph<object>(nodes);
            _graph_CheckIfVisitAll(graph,node=>true);
            graph.Clear();
            _graph_CheckIfVisitAll(graph,node=>node.NodeBase.Id%2==0);
        }
        public void _graph_CheckIfVisitAll<T>(IGraph<T> graph, Func<NodeValue<T>, bool> selector)
        {
            // var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>, object>(1000, 30, 10);
            // var graph = new Graph<object>(nodes);
            // Func<NodeValue<object>, bool> selector = node => true;
            if(selector is null)
                selector = node=>true;
            
            var current_gen = new List<int>();
            var next_gen = new List<int>();
            var buf_gen = new List<int>();

            var visitor = new ActionVisitor<T>(
                (node, visited) =>
                {
                    lock (current_gen)
                    {
                        current_gen.Add(node.NodeBase.Id);
                        if(!visited)
                        node.NodeBase.Children.ForEach(
                            n =>
                            {
                                if (selector(n))
                                    next_gen.Add(n.NodeBase.Id);
                            }
                        );
                    }
                },
                null,
                selector
            );
            graph.AddVisitor(visitor,1,2,3);

            graph.Step();
            buf_gen = next_gen.ToList();
            next_gen.Clear();
            for (int i = 0; i < 50; i++)
            {
                current_gen.Clear();
                next_gen.Clear();
                graph.Step();
                Assert.True(buf_gen.Count==current_gen.Count,$"{i} step. {buf_gen.Count} != {current_gen.Count}");
                buf_gen.Sort();
                current_gen.Sort();
                Assert.Equal(buf_gen, current_gen);
                buf_gen = next_gen.ToList();
            }

        }
        [Fact]
        public void Graph_ValidateOrderAgain()
        {
            var nodes = new Node<object>[10];
            var expectedNodes1 = new int[][]{
                new int[] {0,7},
                new int[] {1,4,9},
                new int[] {2,3,5,8},
                new int[] {0,1,3,6,7,9},
                new int[] {1,2,3,4,7,8,9},
                new int[] {1,2,3,5,7,8,9}
            };

            var expectedNodes2 = new int[][]{
                new int[] {1,5},
                new int[] {0,1,2,3,6,8},
                new int[] {1,2,3,4,7,8,9},
                new int[] {1,2,3,5,7,8,9},
                new int[] {0,1,2,3,6,7,8,9},
                new int[] {1,2,3,4,7,8,9}
            };

            for (int i = 0; i < nodes.Count(); i++)
            {
                nodes[i] = new Node<object>(i);
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

            var graph = new Graph<object>(nodes);
            var visitor1_store = new List<NodeBase<object>>();
            var visitor2_store = new List<NodeBase<object>>();

            ActionVisitor<object> visitor1;
            ActionVisitor<object> visitor2;

            for (int i = 0; i < 20; i++)
            {
                visitor1 = new ActionVisitor<object>((node, visited) =>
                {
                    if (visited) return;
                    lock (visitor1_store) visitor1_store.Add(node.NodeBase);
                }, null, null);

                visitor2 = new ActionVisitor<object>((node, visited) =>
                {
                    if (visited) return;
                    lock (visitor2_store) visitor2_store.Add(node.NodeBase);
                }, null, null);
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
                var nodes = new Node<object>[14];
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i] = new Node<object>(i);
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

                var forward_list = new List<NodeBase<object>>();
                var back_list = new List<NodeBase<object>>();

                var forward_visitor = new ActionVisitor<object>((node, visited) =>
                {
                    if (visited) return;
                    lock (forward_list)
                        forward_list.Add(node.NodeBase);
                },
                null,
                //select happening before vesit
                node =>
                {
                    if (forward_list.Count == 0) return true;
                    return forward_list.Last().Id < node.NodeBase.Id;
                });

                var back_visitor = new ActionVisitor<object>(
                    (node, visited) =>
                    {
                        if (visited) return;
                        lock (back_list)
                            back_list.Add(node.NodeBase);
                    },
                    null,
                    node =>
                    {
                        if (back_list.Count == 0) return true;
                        return back_list.Last().Id > node.NodeBase.Id;
                    });

                var graph = new Graph<object>(nodes);

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
            IEnumerable<Node<object>> nodes = null;
            IGraph<object> graph = null;
            nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>, object>(1000, 30, 70);
            graph = new Graph<object>(nodes);
            validate_graphOrder(graph, nodes, 3, node => node.NodeBase.Id % 2 == 0);

        }
        [Fact]
        public void AddVisitor_ThrowsIfOutOfRange()
        {
            var graph = new Graph<object>(Enumerable.Range(1, 5).Select(i => new Node<object>(i)));
            var visitor = new ActionVisitor<object>((node, v) => { });
            Assert.Throws<IndexOutOfRangeException>(() => graph.AddVisitor(visitor, 22));
        }
        [Fact]
        public void RemoveVisitor_Works()
        {
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>, object>(1000, 30, 70);
            var graph = new Graph<object>(nodes);

            var Children1 = new List<NodeBase<object>>();
            var Children2 = new List<NodeBase<object>>();


            var visitor1 = new ActionVisitor<object>((node, visited) =>
            {
                if (visited) return;
                lock (Children1)
                    Children1.Add(node.NodeBase);
            });
            var visitor2 = new ActionVisitor<object>((node, visited) =>
            {
                if (visited) return;
                lock (Children2)
                    Children2.Add(node.NodeBase);
            });
            graph.AddVisitor(visitor1, 1);
            graph.AddVisitor(visitor2, 2);

            graph.Step();
            Children1.Clear();
            Children2.Clear();

            graph.Step();
            Children1.Sort();
            nodes[1].Children.Sort();
            nodes[2].Children.Sort();

            Assert.Equal(Children1, nodes[1].Children.Select(n => n.NodeBase));
            Assert.Equal(Children2.Count, nodes[2].Children.Count);
            Assert.Equal(Children2, nodes[2].Children.Select(n => n.NodeBase));

            Children1.Clear();
            Children2.Clear();

            graph.RemoveVisitor(visitor1);
            Assert.Throws<KeyNotFoundException>(() => graph.Step(visitor1));
            Children1.Clear();
            Children2.Clear();

            graph.Step();
            Assert.Equal(Children1.Count, 0);
            var __nodes = graph.GetType().GetProperty("_nodes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(graph) as NodeBase<object>[];

            Assert.NotEqual(Children2.Count, 0);

        }
        [Fact]
        public void Step_WrongVisitorThrowsOutOfRangeTrows()
        {

            var graph = new Graph<object>(new List<Node<object>>() { new Node<object>(0), new Node<object>(1), new Node<object>(2), new Node<object>(3) });
            var visitor1 = new ActionVisitor<object>((node, v) => { });
            var visitor2 = new ActionVisitor<object>((node, v) => { });

            graph.AddVisitor(visitor1, 1);

            Assert.Throws<IndexOutOfRangeException>(() =>
                graph.AddVisitor(visitor1, 10));

            Assert.Throws<KeyNotFoundException>(() =>
                graph.Step(visitor2));
        }
        [Fact]
        public void Graph_Vesit_ValidateOrder()
        {
            IEnumerable<Node<object>> nodes = null;
            IGraph<object> graph = null;
            nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>, object>(1000, 30, 70);
            graph = new Graph<object>(nodes);

            validate_graphOrder(graph, nodes, new Random().Next(nodes.Count()));

        }
        [Fact]
        public void Graph_Vesit_ValidateOrderMultipleVisitors()
        {
            const int index1 = 3;
            const int index2 = 9;

            IEnumerable<Node<object>> nodes = null;
            IGraph<object> graph;

            nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>, object>(1000, 30, 70);
            graph = new Graph<object>(nodes);
            validate_graphOrderMultipleVisitors(graph, index1, index2);
        }
        public static void validate_graphOrder(IGraph<object> graph, IEnumerable<NodeBase<object>> nodes, int index, Func<NodeValue<object>, bool> selector = null)
        {
            if (selector is null)
                selector = node => true;
            var next_gen = new HashSet<NodeValue<object>>();
            var current_gen = new List<NodeValue<object>>();
            var buf_gen = new List<NodeValue<object>>();

            var visitor = new ActionVisitor<object>((node, visited) =>
            {
                if (visited) return;
                lock (nodes)
                {
                    current_gen.Add(node);
                    node.NodeBase.Children.ForEach(n =>
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
            buf_gen.Sort();

            next_gen.Clear();
            current_gen.Clear();

            for (int i = 0; i < 50; i++)
            {
                graph.Step();
                current_gen.Sort();
                Assert.True(buf_gen.All(v => selector(v)));
                Assert.Equal(buf_gen.Count, current_gen.Count);
                Assert.Equal(buf_gen, current_gen);

                buf_gen = next_gen.ToList();
                buf_gen.Sort();

                next_gen.Clear();
                current_gen.Clear();
            }
        }
        public static void validate_graphOrderMultipleVisitors(IGraph<object> graph, int index1, int index2, Func<NodeValue<object>, bool> selector = null)
        {
            var next_gen1 = new HashSet<NodeBase<object>>();
            var current_gen1 = new List<NodeBase<object>>();
            var buf_gen1 = new List<NodeBase<object>>();

            var next_gen2 = new HashSet<NodeBase<object>>();
            var current_gen2 = new List<NodeBase<object>>();
            var buf_gen2 = new List<NodeBase<object>>();

            var visitor1 = new ActionVisitor<object>((node, visited) =>
            {
                if (visited) return;
                lock (next_gen1)
                {
                    current_gen1.Add(node.NodeBase);
                    node.NodeBase.Children.ForEach(n =>
                    {
                        if (selector is null)
                            next_gen1.Add(n.NodeBase);
                        else if (selector(n))
                            next_gen1.Add(n.NodeBase);
                    });
                }
            });

            var visitor2 = new ActionVisitor<object>((node, visited) =>
            {
                if (visited) return;
                lock (next_gen2)
                {
                    current_gen2.Add(node.NodeBase);
                    node.NodeBase.Children.ForEach(n =>
                    {
                        if (selector is null)
                            next_gen2.Add(n.NodeBase);
                        else if (selector(n))
                            next_gen2.Add(n.NodeBase);
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
