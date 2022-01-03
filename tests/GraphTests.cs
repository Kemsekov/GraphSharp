
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Edges;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
using Xunit;
namespace tests
{
    public class GraphTests
    {
        NodesFactory _nodes;
        Func<NodesFactory,IGraph> createGraph;
        public GraphTests()
        {
            this._nodes = 
                new NodesFactory()
                .CreateNodes(5000)
                .ForEach()
                .ConnectRandomly(5,30);
            createGraph = nodes=>new Graph(nodes);
            // createGraph = nodes=>new Graph(nodes,PropagatorFactory.Create<Propagator>());
        }

        [Fact]
        public void ValidateOrderWithManualData()
        {
            var nodes = new Node[10];
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
                nodes[i] = new Node(i);
            }
            //init graph with nodes
            {
                nodes[0].Edges.Add(new Edge(nodes[4]));
                nodes[0].Edges.Add(new Edge(nodes[1]));

                nodes[1].Edges.Add(new Edge(nodes[2]));
                nodes[1].Edges.Add(new Edge(nodes[3]));
                nodes[1].Edges.Add(new Edge(nodes[8]));

                nodes[2].Edges.Add(new Edge(nodes[7]));
                nodes[2].Edges.Add(new Edge(nodes[9]));

                nodes[3].Edges.Add(new Edge(nodes[7]));

                nodes[4].Edges.Add(new Edge(nodes[5]));

                nodes[5].Edges.Add(new Edge(nodes[0]));
                nodes[5].Edges.Add(new Edge(nodes[1]));
                nodes[5].Edges.Add(new Edge(nodes[3]));
                nodes[5].Edges.Add(new Edge(nodes[6]));

                nodes[6].Edges.Add(new Edge(nodes[3]));

                nodes[7].Edges.Add(new Edge(nodes[1]));
                nodes[7].Edges.Add(new Edge(nodes[9]));

                nodes[8].Edges.Add(new Edge(nodes[9]));

                nodes[9].Edges.Add(new Edge(nodes[3]));
            }

            var graph = createGraph(new NodesFactory().UseNodes(nodes));
            var visitor1_store = new List<INode>();
            var visitor2_store = new List<INode>();

            ActionVisitor visitor1;
            ActionVisitor visitor2;

            for (int i = 0; i < 20; i++)
            {
                visitor1 = new ActionVisitor(node =>
                {
                    lock (visitor1_store) visitor1_store.Add(node);
                }, null, null);

                visitor2 = new ActionVisitor(node =>
                {
                    lock (visitor2_store) visitor2_store.Add(node);
                }, null, null);
                graph.RemoveAllVisitors();

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

                graph.RemoveAllVisitors();

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
        public void ValidateOrder(){
            var graph = createGraph(_nodes);
            validate_graphOrder(graph,_nodes.Nodes,edge=>true);
        }
        static void validate_graphOrder(IGraph graph, IList<INode> nodes, Func<IEdge, bool> selector = null)
        {
            var next_gen = new HashSet<INode>();
            var current_gen = new List<INode>();
            var buf_gen = new List<INode>();

            var visitor = new ActionVisitor(node =>
            {
                lock (current_gen)
                {
                    current_gen.Add(node);
                    foreach (var n in node.Edges)
                    {
                        if (selector is null)
                            next_gen.Add(n.Node);
                        else if (selector(n))
                            next_gen.Add(n.Node);
                    }
                }
            }, selector);

            graph.AddVisitor(visitor, 1,2,3,4);

            graph.Step();
            buf_gen = next_gen.ToList();
            buf_gen.Sort();

            next_gen.Clear();
            current_gen.Clear();

            for (int i = 0; i < 50; i++)
            {
                graph.Step();
                current_gen.Sort();
                Assert.True(buf_gen.Distinct().Count()==current_gen.Count(),"There is copies in buffer");
                Assert.Equal(buf_gen.Count, current_gen.Count);
                Assert.Equal(buf_gen, current_gen);

                buf_gen = next_gen.ToList();
                buf_gen.Sort();

                next_gen.Clear();
                current_gen.Clear();
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
                        nodes[i].Edges.Add(new Edge(nodes[i + 1]));
                    }
                }

                for (int i = nodes.Length - 1; i > 0; i--)
                {
                    if (i - 1 >= 0)
                    {
                        nodes[i].Edges.Add(new Edge(nodes[i - 1]));
                    }
                }

                var forward_list = new List<INode>();
                var back_list = new List<INode>();

                var forward_visitor = new ActionVisitor(node =>
                {

                    lock (forward_list)
                        forward_list.Add(node);
                },
                //select happening before vesit
                node =>
                {
                    if (forward_list.Count == 0) return true;
                    return forward_list.Last().Id < node.Node.Id;
                });

                var back_visitor = new ActionVisitor(
                    node =>
                    {

                        lock (back_list)
                            back_list.Add(node);
                    },
                    node =>
                    {
                        if (back_list.Count == 0) return true;
                        return back_list.Last().Id > node.Node.Id;
                    });

                var graph = createGraph(new NodesFactory().UseNodes(nodes));

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
        public void RemoveVisitor_Works()
        {
            var graph = createGraph(_nodes);
            var visitor1 = new ActionVisitor(node => { });
            var visitor2 = new ActionVisitor(node => { Assert.True(false); });
            graph.AddVisitor(visitor1, 1);
            graph.AddVisitor(visitor2, 2);
            graph.RemoveVisitor(visitor2);
            graph.Step();
        }
        [Fact]
        public void Step_WrongVisitorThrows()
        {
            var graph = createGraph(_nodes);
            var visitor1 = new ActionVisitor(node => { });
            var visitor2 = new ActionVisitor(node => { });

            graph.AddVisitor(visitor1, 1);

            Assert.Throws<KeyNotFoundException>(() =>
                graph.Step(visitor2));
        }
    }
}
