using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
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
        Func<IVisitor<TestNode,TestEdge>, IPropagator<TestNode>>[] _propagatorFactories;
        GraphStructure<TestNode, TestEdge> _nodes;

        public PropagatorTests()
        {
            _propagatorFactories = new Func<IVisitor<TestNode,TestEdge>, IPropagator<TestNode>>[2];
            _propagatorFactories[0] = visitor => new Propagator<TestNode,TestEdge>(visitor);
            _propagatorFactories[1] = visitor => new ParallelPropagator<TestNode,TestEdge>(visitor);
            _nodes = new GraphStructure<TestNode, TestEdge>(new TestGraphConfiguration()).CreateNodes(1000);
            _nodes.ForEach().ConnectNodes(10);
        }

        [Fact]
        public void SetPosition_SetNodes_Works()
        {
            foreach (var factory in _propagatorFactories)
            {
                var visitedNodes = new List<INode>();
                var visitor = new ActionVisitor<TestNode,TestEdge>(
                    (node) =>
                    {
                        lock (visitedNodes)
                            visitedNodes.Add(node);
                    },
                    (edge) => true,
                    () => visitedNodes.Sort());
                var propagator = factory(visitor);
                propagator.SetNodes(_nodes);
                propagator.SetPosition(1, 2);
                propagator.Propagate();

                Assert.Equal(visitedNodes, new[] { _nodes.Nodes[1], _nodes.Nodes[2] });
                visitedNodes.Clear();
                propagator.SetPosition(5, 6);

                propagator.Propagate();
                Assert.Equal(visitedNodes, new[] { _nodes.Nodes[5], _nodes.Nodes[6] });
                visitedNodes.Clear();
            }
        }
        [Fact]
        public void Propagate_SelectWorks()
        {
            var visited = new List<INode>();
            foreach (var factory in _propagatorFactories)
            {
                visited.Clear();
                var visitor = new ActionVisitor<TestNode,TestEdge>(
                    n => visited.Add(n),
                    e => e.Child.Id % 2 == 0);
                var propagator = factory(visitor);
                propagator.SetNodes(_nodes);
                propagator.SetPosition(0, 1, 2, 3, 4, 5);
                propagator.Propagate();
                visited.Sort();
                Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, visited.Select(x => x.Id));
                for(int i = 0;i<5;i++){
                    visited.Clear();
                    propagator.Propagate();
                    foreach(var a in visited)
                    Assert.True(a.Id%2==0);
                }

            }
        }
        [Fact]
        public void Propagate_DoesNotVisitTwice()
        {
            var visited = new List<INode>();
            var rand = new Random();
            var randNodeId = () => rand.Next(_nodes.Nodes.Count());
            foreach (var factory in _propagatorFactories)
            {

                var visitor = new ActionVisitor<TestNode,TestEdge>(
                    n =>
                    {
                        lock (visited)
                            visited.Add(n);
                    },
                    e =>
                    {
                        return true;
                    });
                var propagator = factory(visitor);
                propagator.SetNodes(_nodes);
                propagator.SetPosition(randNodeId(), randNodeId(), randNodeId());

                for (int i = 0; i < 10; i++)
                {
                    visited.Clear();
                    propagator.Propagate();
                    Assert.Equal(visited, visited.Distinct(new NodeEqualityComparer()));
                }
            }
        }
        [Fact]
        public void Propagate_HaveRightNodesVisitOrder()
        {
            var visited = new List<INode>();
            var expected = new SortedSet<INode>(new NodesComparer());
            var rand = new Random();
            var randNode = () => _nodes.Nodes[rand.Next(_nodes.Nodes.Count())];
            foreach (var factory in _propagatorFactories)
            {

                var visitor = new ActionVisitor<TestNode,TestEdge>(
                    n =>
                    {
                        lock (visited)
                            visited.Add(n);
                    },
                    e =>
                    {
                        lock (expected)
                            foreach (var n in e.Child.Edges)
                                expected.Add(n.Child);
                        return true;
                    });
                var propagator = factory(visitor);
                propagator.SetNodes(_nodes);
                for (int i = 0; i < 5; i++)
                {
                    expected.Add(randNode());
                } 
                propagator.SetPosition(expected.Select(x => x.Id).ToArray());
                
                List<INode> buf;
                for (int i = 0; i < 20; i++)
                {
                    buf = expected.ToList();
                    expected.Clear();
                    propagator.Propagate();
                    //first time when we call Propagate it will not call Select
                    //on visitor so in order to keep track of called nodes we will do it
                    //manually
                    if(i==0){
                        foreach(var e in buf)
                            visitor.Select(new TestEdge(null,e as TestNode));
                    }
                    visited.Sort();
                    Assert.Equal(buf.Select(x => x.Id), visited.Select(x => x.Id));
                    visited.Clear();
                }
            }
        }
        [Fact]
        public void Propagate_HaveRightNodesVisitOrderWithManualData()
        {
            var nodes = new GraphStructure<TestNode, TestEdge>(new TestGraphConfiguration())
                .CreateNodes(10);
            foreach (var pair in ManualTestData.NodesConnections)
            {
                nodes.Nodes[pair[0]].Edges.Add(new TestEdge(null,nodes.Nodes[pair[1]]));
            }
            var actualValues = new List<int>();
            var visitor = new ActionVisitor<TestNode,TestEdge>(
                x =>
                {
                    lock (actualValues)
                        actualValues.Add(x.Id);
                },
                e => true);
            for(int c = 0;c<10;c++)
            foreach (var expectedValues in ManualTestData.ExpectedOrder)
                foreach (var factory in _propagatorFactories)
                {
                    var propagator = factory(visitor);
                    propagator.SetNodes(nodes);
                    propagator.SetPosition(expectedValues[0]);

                    for (int i = 0; i < expectedValues.Length; i++)
                    {
                        actualValues.Clear();
                        propagator.Propagate();
                        actualValues.Sort();
                        Assert.Equal(actualValues, expectedValues[i]);
                    }
                }
        }
    }
}