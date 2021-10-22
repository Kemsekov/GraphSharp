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
    public class GraphTests_dev
    {
        [Fact]
        public void Graph_ValidateVisitOrder()
        {
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node, NodeBase>(1000, 30, 10);
            var graph = new Graph(nodes);

            Func<List<INode>, List<INode>, Func<NodeBase, bool>, IVisitor> create_visitor =
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
                graph.AddVisitor(visitor);
                return visitor;
            };

            validateVisitOrderForOneVisit(graph, create_visitor, node => true);
            validateVisitOrderForOneVisit(graph, create_visitor, node => node.Id % 2 == 0);

        }
        [Fact]
        public void GraphGeneric_ValidateVisitOrder()
        {
            var nodes_g = NodeGraphFactory.CreateRandomConnectedParallel<Node<object>, NodeValue<object>>(1000, 30, 10);
            var graph_g = new Graph<object>(nodes_g);

            bool skip_if_visited = false;

            Func<List<INode>, List<INode>, Func<NodeValue<object>, bool>, IVisitor<object>> create_visitor_g =
            (current_gen, next_gen, selector) =>
            {
                var visitor = new ActionVisitor<object>(
                    (node, visited) =>
                    {
                        if (skip_if_visited && visited) return;
                        lock (current_gen)
                        {
                            current_gen.Add(node.NodeBase);
                            if (!visited)
                                node.NodeBase.Children.ForEach(
                                    n =>
                                    {
                                        if (selector(n))
                                            next_gen.Add(n.NodeBase);
                                    }
                                );
                        }
                    },
                    null,
                    selector
                );
                graph_g.AddVisitor(visitor);
                return visitor;
            };

            validtaeVisitOrderForMultipleVisit(graph_g, create_visitor_g, nodeValue => true);
            validtaeVisitOrderForMultipleVisit(graph_g, create_visitor_g, nodeValue => nodeValue.NodeBase.Id % 2 == 0);

            skip_if_visited = true;
            validateVisitOrderForOneVisit(graph_g, create_visitor_g, nodeValue => true);
            validateVisitOrderForOneVisit(graph_g, create_visitor_g, nodeValue => nodeValue.NodeBase.Id % 2 == 0);

        }

        private void validateVisitOrderForOneVisit<TChild, TVisitor>(
            IGraphShared<TChild, TVisitor> graph,
            Func<List<INode>, List<INode>, Func<TChild, bool>, TVisitor> create_visitor,
            Func<TChild, bool> selector)
        where TVisitor : IVisitorShared<TChild>
        where TChild : IChild
        {
            var current_gen = new List<INode>();
            var next_gen = new List<INode>();
            List<INode> buf_gen = null;

            var visitor = create_visitor.Invoke(current_gen, next_gen, selector);
            graph.Step(visitor);
            buf_gen = next_gen.Distinct().ToList();
            for (int i = 0; i < 50; i++)
            {
                current_gen.Clear();
                next_gen.Clear();
                graph.Step(visitor);
                buf_gen.Sort();
                current_gen.Sort();
                Assert.True(current_gen.Count == buf_gen.Count, $"{current_gen.Count} != {buf_gen.Count} on step {i}");
                Assert.Equal(current_gen, buf_gen);
                buf_gen = next_gen.Distinct().ToList();
            }
        }
        private void validtaeVisitOrderForMultipleVisit<TChild, TVisitor>(
            IGraphShared<TChild, TVisitor> graph,
            Func<List<INode>, List<INode>, Func<TChild, bool>, TVisitor> create_visitor,
            Func<TChild, bool> selector
        )
        where TVisitor : IVisitorShared<TChild>
        where TChild : IChild
        {
            var current_gen = new List<INode>();
            var next_gen = new List<INode>();
            List<INode> buf_gen = null;

            var visitor = create_visitor.Invoke(current_gen, next_gen, selector);
            graph.Step(visitor);
            buf_gen = next_gen.ToList();
            for (int i = 0; i < 50; i++)
            {
                current_gen.Clear();
                next_gen.Clear();
                graph.Step(visitor);
                buf_gen.Sort();
                current_gen.Sort();
                Assert.True(current_gen.Count == buf_gen.Count, $"{current_gen.Count} != {buf_gen.Count} on step {i}");
                Assert.Equal(current_gen, buf_gen);
                buf_gen = next_gen.ToList();
            }

        }
    }
}