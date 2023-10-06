using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GraphSharp.Common;
using GraphSharp.Graphs;

using GraphSharp.Propagators;
using GraphSharp.Tests.helpers;
using GraphSharp.Tests.Helpers;
using GraphSharp.Tests.Models;
using GraphSharp.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace GraphSharp.Tests
{
    public class PropagatorTests
    {
        Func<IVisitor<Edge>, bool, PropagatorBase<Edge>>[] _propagatorFactories;
        IGraph<Node, Edge> _graph;

        public PropagatorTests()
        {
            _graph = new Graph<Node, Edge>(new TestGraphConfiguration(new()));
            _graph.Do.CreateNodes(1000);
            _graph.Do.ConnectNodes(10);

            _propagatorFactories = new Func<IVisitor<Edge>, bool, PropagatorBase<Edge>>[2];
            _propagatorFactories[0] = (visitor, reversed) =>
            {
                var p = new Propagator<Edge>(_graph.Edges,visitor);
                if (reversed)
                {
                    p.SetToIterateByInEdges();
                }
                return p;
            };
            _propagatorFactories[1] = (visitor, reversed) =>
            {
                var p = new ParallelPropagator<Edge>(_graph.Edges,visitor);
                if (reversed)
                {
                    p.SetToIterateByInEdges();
                }
                return p;
            };
        }

        [Fact]
        public void RetainsStatesBetweenIterations()
        {
            var states = new Dictionary<int, byte>();

            void checkStates(PropagatorBase<Edge> propagator)
            {
                foreach (var state in states)
                {
                    Assert.True(propagator.NodeStates.IsInState(state.Value, state.Key));
                }
            }

            foreach (var n in _graph.Nodes)
            {
                states[n.Id] = (byte)Math.Pow(2, (Random.Shared.Next(6) + 2));
            }
            for (bool reversed = false; !reversed; reversed = true)
                foreach (var factory in _propagatorFactories)
                {
                    var visitor = new ActionVisitor<Node, Edge>(
                        n => { },
                        e => true);
                    var propagator = factory(visitor, reversed);
                    propagator.SetPosition(0, 1, 2, 3, 4, 5);
                    foreach (var state in states)
                    {
                        propagator.NodeStates.AddState(state.Value, state.Key);
                    }
                    checkStates(propagator);
                    for (int i = 0; i < 50; i++)
                        propagator.Propagate();
                    checkStates(propagator);
                }
        }

        [Fact]
        public void SetPosition_Works()
        {
            for (bool reversed = false; !reversed; reversed = true)
                foreach (var factory in _propagatorFactories)
                {
                    var visitedNodes = new List<int>();
                    var visitor = new ActionVisitor<Node, Edge>(
                        (node) =>
                        {
                            lock (visitedNodes)
                                visitedNodes.Add(node);
                        },
                        (edge) => true,
                        () => visitedNodes.Sort());
                    var propagator = factory(visitor, reversed);
                    foreach (var n in _graph.Nodes)
                    {
                        if (n.Id % 3 == 0)
                            propagator.NodeStates.AddState(32, n.Id);
                    }
                    propagator.SetPosition(1, 2);

                    if (reversed) _graph.Do.ReverseEdges();
                    propagator.Propagate();

                    Assert.Equal(visitedNodes, new[] { 1, 2 });
                    visitedNodes.Clear();
                    propagator.SetPosition(5, 6);

                    propagator.Propagate();
                    Assert.Equal(visitedNodes, new[] { 5, 6 });
                    visitedNodes.Clear();
                    Assert.True(_graph.Nodes.Where(x => x.Id % 3 == 0).All(x => propagator.NodeStates.IsInState(32, x.Id)));
                }
        }
        [Fact]
        public void Propagate_RightVisitorMethodsOrderExecution()
        {
            for (bool reversed = false; !reversed; reversed = true)
                foreach (var factory in _propagatorFactories)
                {
                    var order = new List<int>();
                    var visitor = new ActionVisitor<Node, Edge>(
                        visit: node =>
                        {
                            lock (order)
                                if (order.Last() != 2)
                                    order.Add(2);
                        },
                        select: edge =>
                        {
                            lock (order)
                                if (order.Last() != 1)
                                    order.Add(1);
                            return true;
                        },
                        start: () => order.Add(0),
                        end: () => order.Add(3)
                    );
                    var propagator = factory(visitor, reversed);
                    propagator.SetPosition(1, 2, 3);
                    if (reversed) this._graph.Do.ReverseEdges();
                    propagator.Propagate();
                    Assert.False(order.Contains(1));
                    Assert.Equal(order, order.OrderBy(x => x));
                    for (int i = 0; i < 10; i++)
                    {
                        order.Clear();
                        propagator.Propagate();
                        Assert.Equal(order, order.OrderBy(x => x));
                    }
                }
        }
        [Fact]
        public void Propagate_SelectWorks()
        {
            var visited = new List<int>();
            for (bool reversed = false; !reversed; reversed = true)
                foreach (var factory in _propagatorFactories)
                {
                    visited.Clear();
                    var visitor = new ActionVisitor<Node, Edge>(
                        n => visited.Add(n),
                        e => e.TargetId % 2 == 0);
                    var propagator = factory(visitor, reversed);
                    propagator.SetPosition(0, 1, 2, 3, 4, 5);
                    if (reversed) _graph.Do.ReverseEdges();
                    propagator.Propagate();
                    visited.Sort();
                    Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, visited);
                    for (int i = 0; i < 5; i++)
                    {
                        visited.Clear();
                        propagator.Propagate();
                        foreach (var a in visited)
                            Assert.True(a % 2 == 0);
                    }

                }
        }
        [Fact]
        public void Propagate_DoesNotVisitTwice()
        {
            var visited = new List<int>();
            var rand = new Random();
            var randNodeId = () => rand.Next(_graph.Nodes.Count());
            for (bool reversed = false; !reversed; reversed = true)
                foreach (var factory in _propagatorFactories)
                {

                    var visitor = new ActionVisitor<Node, Edge>(
                        n =>
                        {
                            lock (visited)
                                visited.Add(n);
                        },
                        e =>
                        {
                            return true;
                        });
                    var propagator = factory(visitor, reversed);
                    propagator.SetPosition(randNodeId(), randNodeId(), randNodeId());
                    if (reversed) _graph.Do.ReverseEdges();

                    for (int i = 0; i < 10; i++)
                    {
                        visited.Clear();
                        propagator.Propagate();
                        Assert.Equal(visited, visited.Distinct());
                    }
                }
        }
        [Fact]
        public void Propagate_HaveRightNodesVisitOrder()
        {
            var visited = new List<int>();
            var expected = new SortedSet<int>();
            var rand = new Random();
            var randNode = () => rand.Next(_graph.Nodes.Count());
            for (bool reversed = false; !reversed; reversed = true)
                foreach (var factory in _propagatorFactories)
                {

                    var visitor = new ActionVisitor<Node, Edge>(
                        n =>
                        {
                            lock (visited)
                                visited.Add(n);
                        },
                        e =>
                        {
                            lock (expected)
                                foreach (var n in _graph.Edges.OutEdges(e.TargetId))
                                    expected.Add(n.TargetId);
                            return true;
                        });
                    var propagator = factory(visitor, reversed);
                    if (reversed) _graph.Do.ReverseEdges();
                    for (int i = 0; i < 5; i++)
                    {
                        expected.Add(randNode());
                    }
                    propagator.SetPosition(expected.ToArray());

                    List<int> buf;
                    for (int i = 0; i < 20; i++)
                    {
                        buf = expected.ToList();
                        expected.Clear();
                        propagator.Propagate();
                        //first time when we call Propagate it will not call Select
                        //on visitor so in order to keep track of called nodes we will do it
                        //manually
                        if (i == 0)
                        {
                            foreach (var e in buf)
                                visitor.Select(new(new Edge(-1, e),-1));
                        }
                        visited.Sort();
                        Assert.Equal(buf.Count, visited.Count);
                        Assert.Equal(buf, visited);
                        visited.Clear();
                    }
                }
        }
        [Fact]
        public void Propagate_HaveRightNodesVisitOrderWithManualData()
        {
            var graph = new Graph<Node, Edge>(new TestGraphConfiguration(new()));
            graph.Do.CreateNodes(10);
            foreach (var pair in ManualTestData.NodesConnections)
            {
                graph.Edges.Add(new Edge(graph.Nodes[pair[0]], graph.Nodes[pair[1]]));
            }

            var actualValues = new List<int>();
            var visitor = new ActionVisitor<Node, Edge>(
                x =>
                {
                    lock (actualValues)
                        actualValues.Add(x);
                },
                e => true);
            var reversedGraph = graph.Clone();
            reversedGraph.Do.ReverseEdges();

            for (int c = 0; c < 10; c++)
                foreach (var expectedValues in ManualTestData.ExpectedOrder)
                    for (bool reversed = false; !reversed; reversed = true)
                        foreach (var factory in _propagatorFactories)
                        {
                            var propagator = factory(visitor, reversed);
                            if (reversed)
                                propagator.Reset(reversedGraph.Edges, visitor);
                            else
                                propagator.Reset(graph.Edges, visitor);
                            propagator.SetPosition(expectedValues[0]);
                            for (int i = 0; i < expectedValues.Length; i++)
                            {
                                actualValues.Clear();
                                propagator.Propagate();
                                actualValues.Sort();
                                Assert.Equal(expectedValues[i], actualValues);
                            }
                        }
        }
        [Fact]
        public void ReverseOrder_HaveRightVisitOrder()
        {
            _graph.Do.CreateNodes(1000).ConnectRandomly(2, 10);
            foreach (var factory in _propagatorFactories)
            {
                var v = new ActionVisitor<Node, Edge>();
                var p1 = factory(v, false);
                var p2 = factory(v, true);

                p1.SetPosition(1, 2);
                p2.SetPosition(1, 2);
                p2.SetToIterateByInEdges();


                var order = new List<List<int>>();
                v.VisitEvent += node =>
                {
                    var list = order.Last();
                    lock (list)
                        list.Add(node);
                };
                v.StartEvent += () => order.Add(new List<int>());
                v.EndEvent += () => order.Last().Sort();

                for (int i = 0; i < 10; i++)
                {
                    p1.Propagate();
                }
                var straightOrder = order.ToList();
                order.Clear();
                _graph.Do.ReverseEdges();
                for (int i = 0; i < 10; i++)
                {
                    p2.Propagate();
                }

                var reverseOrder = order.ToList();
                Assert.Equal(straightOrder.Count, reverseOrder.Count);
                foreach (var u in straightOrder.Zip(reverseOrder))
                {
                    u.First.Sort();
                    u.Second.Sort();
                    Assert.Equal(u.First, u.Second);
                }
            }
        }
        [Fact]
        public void Reset_Works()
        {

            foreach (var factory in _propagatorFactories)
            {
                var visitor1 = new ActionVisitor<Node, Edge>();
                var visitor2 = new ActionVisitor<Node, Edge>();
                var propagator = factory.Invoke(visitor1, false);
                Assert.True(_graph.Nodes.All(x => propagator.NodeStates.GetState(x.Id) == UsedNodeStates.IterateByOutEdges));
                propagator.SetToIterateByBothEdges();
                Assert.True(_graph.Nodes.All(x => propagator.NodeStates.GetState(x.Id) == (UsedNodeStates.IterateByOutEdges | UsedNodeStates.IterateByInEdges)));
                propagator.Reset(_graph.Edges, visitor2);
                Assert.Equal(propagator.Visitor, visitor2);
                Assert.True(_graph.Nodes.All(x => propagator.NodeStates.GetState(x.Id) == UsedNodeStates.IterateByOutEdges));
            }
        }
        [Fact]
        public void IterationChangeFunctions_Works()
        {
            foreach (var factory in _propagatorFactories)
            {
                var visitor = new ActionVisitor<Node, Edge>();

                var propagator = factory.Invoke(visitor, false);
                Assert.True(_graph.Nodes.All(x => propagator.NodeStates.GetState(x.Id) == UsedNodeStates.IterateByOutEdges));
                foreach (var n in _graph.Nodes)
                {
                    if (n.Id % 3 == 0)
                        propagator.NodeStates.AddState(32, n.Id);
                }
                Assert.True(_graph.Nodes.All(x => propagator.NodeStates.IsInState(UsedNodeStates.IterateByOutEdges, x.Id)));
                propagator.SetToIterateByInEdges();
                Assert.True(_graph.Nodes.All(x => propagator.NodeStates.IsInState(UsedNodeStates.IterateByInEdges, x.Id)));
                propagator.SetToIterateByOutEdges();
                Assert.True(_graph.Nodes.All(x => propagator.NodeStates.IsInState(UsedNodeStates.IterateByOutEdges, x.Id)));
                propagator.SetToIterateByBothEdges();
                Assert.True(_graph.Nodes.All(x => propagator.NodeStates.IsInState((UsedNodeStates.IterateByOutEdges | UsedNodeStates.IterateByInEdges), x.Id)));
                Assert.True(_graph.Nodes.Where(x => x.Id % 3 == 0).All(x => propagator.NodeStates.IsInState(32, x.Id)));
            }
        }
    }
}