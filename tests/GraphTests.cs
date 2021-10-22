using GraphSharp.Nodes;
using GraphSharp;
using Xunit;
using GraphSharp.Graphs;
using GraphSharp.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace tests
{
    public class GraphTests
    {

        [Fact]
        public void Graph_EndVisitWork()
        {
            var nodes = NodeGraphFactory.CreateConnected<Node, NodeBase>(200, 5);
            int counter = 0;
            var visitor = new ActionVisitor(
                node => { },
                () => counter++,
                node => true
            );
            var graph = new Graph(nodes);
            graph.AddVisitor(visitor, 1, 2, 3);
            for (int i = 0; i < 50; i++)
            {
                Assert.Equal(counter, i);
                graph.Step();
            }
        }
        [Fact]
        public void Graph_ValidateOrderWithManualData()
        {
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

            var nodes = new Node[10];

            for (int i = 0; i < nodes.Count(); i++)
            {
                nodes[i] = new Node(i);
            }
            InitNodes();
            var graph = new Graph(nodes);
            Func<List<NodeBase>, Func<NodeBase, bool>, IVisitor> t = (current_gen, selector) =>
            {
                var visitor = new ActionVisitor(
                    node =>
                    {
                        lock (current_gen)
                            current_gen.Add(node);
                    },
                    null,
                    selector
                );
                return visitor;
            };
            validateVisitWithManualData(graph, t, nodeValue => true, expectedNodes1.Select(ids => ids.Select(id => nodes[id]).ToArray()).ToArray(), 0, 7);
            validateVisitWithManualData(graph, t, nodeValue => true, expectedNodes2.Select(ids => ids.Select(id => nodes[id]).ToArray()).ToArray(), 1, 5);

            var nodes_g =
                nodes.Select(n => new Node<object>(n.Id)).ToArray();

            for (int i = 0; i < nodes_g.Length; i++)
            {
                nodes_g[i].Children.AddRange(
                    nodes[i].Children.Select(n => new NodeValue<object>(nodes_g[n.Id], new Object()))
                );
            }
            var graph_g = new Graph<object>(nodes_g);
            Func<List<NodeValue<object>>, Func<NodeValue<object>, bool>, IVisitor<object>> t_g = (current_gen, selector) =>
            {
                var visitor = new ActionVisitor<object>(
                    (nodeValue, visited) =>
                    {
                        lock (current_gen)
                        {
                            if (!visited)
                                current_gen.Add(nodeValue);
                        }
                    },
                    null,
                    selector
                );
                return visitor;
            };
            validateVisitWithManualData(graph_g, t_g, nodeValue => true, expectedNodes1.Select(ids => ids.Select(id => new NodeValue<object>(nodes_g[id], new Object())).ToArray()).ToArray(), 0, 7);
            validateVisitWithManualData(graph_g, t_g, nodeValue => true, expectedNodes2.Select(ids => ids.Select(id => new NodeValue<object>(nodes_g[id], new Object())).ToArray()).ToArray(), 1, 5);

            void InitNodes()
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
        }
        private void validateVisitWithManualData<TChild, TVisitor>(
            IGraphShared<TChild, TVisitor> graph,
            Func<List<TChild>, Func<TChild, bool>, TVisitor> create_visited,
            Func<TChild, bool> selector,
            TChild[][] expected_values,
            params int[] indices
        )
        where TVisitor : IVisitorShared<TChild>
        where TChild : IChild
        {
            var current_gen = new List<TChild>();
            var visitor = create_visited(current_gen, selector);
            graph.AddVisitor(visitor, indices);
            foreach (var value in expected_values)
            {
                current_gen.Clear();
                graph.Step();
                current_gen.Sort();
                Assert.Equal(current_gen, value);
            }
            graph.RemoveVisitor(visitor);
        }
        [Fact]
        public void Graph_ValidateVisitOrder()
        {
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node, NodeBase>(1000, 30, 10);
            var graph = new Graph(nodes);

            Func<List<NodeBase>, List<NodeBase>, Func<NodeBase, bool>, IVisitor> create_visitor =
            (current_gen, next_gen, selector) =>
            {
                var visitor = new ActionVisitor(
                    node =>
                    {
                        lock (current_gen)
                        {
                            current_gen.Add(node);
                            node.Children.ForEach(
                                n =>
                                {
                                    if (selector(n))
                                        next_gen.Add(n);
                                }
                            );
                        }
                    },
                    null,
                    selector
                );
                return visitor;
            };

            validateVisitOrderForOneVisit(graph, create_visitor, node => true, 2, 5, 7);
            validateVisitOrderForOneVisit(graph, create_visitor, node => node.Id % 2 == 0, 8, 9, 6);

        }
        [Fact]
        public void GraphGeneric_ValidateVisitOrder()
        {
            var nodes_g = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>, NodeValue<object>>(1000, 30, 10);
            var graph_g = new Graph<object>(nodes_g);

            bool skip_if_visited = false;

            Func<List<NodeValue<object>>, List<NodeValue<object>>, Func<NodeValue<object>, bool>, IVisitor<object>> create_visitor_g =
            (current_gen, next_gen, selector) =>
            {
                var visitor = new ActionVisitor<object>(
                    (node, visited) =>
                    {
                        if (skip_if_visited && visited) return;
                        lock (current_gen)
                        {
                            current_gen.Add(node);
                            if (!visited)
                                node.NodeBase.Children.ForEach(
                                    n =>
                                    {
                                        if (selector(n))
                                            next_gen.Add(n);
                                    }
                                );
                        }
                    },
                    null,
                    selector
                );
                return visitor;
            };

            validtaeVisitOrderForMultipleVisit(graph_g, create_visitor_g, nodeValue => true, 1, 2, 3);
            validtaeVisitOrderForMultipleVisit(graph_g, create_visitor_g, nodeValue => nodeValue.NodeBase.Id % 2 == 0, 3, 4, 5);

            skip_if_visited = true;
            validateVisitOrderForOneVisit(graph_g, create_visitor_g, nodeValue => true, 5, 6, 7);
            validateVisitOrderForOneVisit(graph_g, create_visitor_g, nodeValue => nodeValue.NodeBase.Id % 2 == 0, 7, 8, 9);

        }
        private void validateVisitOrderForOneVisit<TChild, TVisitor>(
            IGraphShared<TChild, TVisitor> graph,
            Func<List<TChild>, List<TChild>, Func<TChild, bool>, TVisitor> create_visitor,
            Func<TChild, bool> selector,
            params int[] indices
        )
        where TVisitor : IVisitorShared<TChild>
        where TChild : IChild
        {
            var current_gen = new List<TChild>();
            var next_gen = new List<TChild>();
            List<TChild> buf_gen = null;

            var visitor = create_visitor.Invoke(current_gen, next_gen, selector);
            graph.AddVisitor(visitor, indices);
            graph.Step();
            buf_gen = next_gen.Distinct().ToList();
            for (int i = 0; i < 50; i++)
            {
                current_gen.Clear();
                next_gen.Clear();
                graph.Step();
                buf_gen.Sort();
                current_gen.Sort();
                Assert.True(current_gen.Count == buf_gen.Count, $"{current_gen.Count} != {buf_gen.Count} on step {i}");
                Assert.Equal(current_gen, buf_gen);
                buf_gen = next_gen.Distinct().ToList();
            }
            graph.RemoveVisitor(visitor);
        }
        private void validtaeVisitOrderForMultipleVisit<TChild, TVisitor>(
            IGraphShared<TChild, TVisitor> graph,
            Func<List<TChild>, List<TChild>, Func<TChild, bool>, TVisitor> create_visitor,
            Func<TChild, bool> selector,
            params int[] indices
        )
        where TVisitor : IVisitorShared<TChild>
        where TChild : IChild
        {
            var current_gen = new List<TChild>();
            var next_gen = new List<TChild>();
            List<TChild> buf_gen = null;

            var visitor = create_visitor.Invoke(current_gen, next_gen, selector);
            graph.AddVisitor(visitor, indices);
            graph.Step();
            buf_gen = next_gen.ToList();
            for (int i = 0; i < 50; i++)
            {
                current_gen.Clear();
                next_gen.Clear();
                graph.Step();
                buf_gen.Sort();
                current_gen.Sort();
                Assert.True(current_gen.Count == buf_gen.Count, $"{current_gen.Count} != {buf_gen.Count} on step {i}");
                Assert.Equal(current_gen, buf_gen);
                buf_gen = next_gen.ToList();
            }
            graph.RemoveVisitor(visitor);
        }
    }
}