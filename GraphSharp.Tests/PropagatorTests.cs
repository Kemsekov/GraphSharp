using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        Func<IVisitor<Node,Edge>, PropagatorBase<Node,Edge>>[] _propagatorFactories;
        Graph<Node, Edge> _graph;

        public PropagatorTests()
        {
            _graph = new Graph<Node, Edge>(new TestGraphConfiguration(new())).Create(1000);
            _graph.Do.ConnectNodes(10);

            _propagatorFactories = new Func<IVisitor<Node,Edge>, PropagatorBase<Node,Edge>>[2];
            _propagatorFactories[0] = visitor => new Propagator<Node,Edge>(visitor,_graph);
            _propagatorFactories[1] = visitor => new ParallelPropagator<Node,Edge>(visitor,_graph);
        }

        [Fact]
        public void RetainsStatesBetweenIterations(){
            var states = new Dictionary<int,byte>();

            void checkStates(PropagatorBase<Node,Edge> propagator){
                foreach(var state in states){
                    Assert.True(propagator.IsNodeInState(state.Key,state.Value));
                }
            }

            foreach(var n in _graph.Nodes){
                states[n.Id]=(byte)Math.Pow(2,(Random.Shared.Next(6)+2));
            }
            foreach (var factory in _propagatorFactories)
            {
                var visitor = new ActionVisitor<Node,Edge>(
                    n => {},
                    e => true);
                var propagator = factory(visitor);
                propagator.SetPosition(0, 1, 2, 3, 4, 5);
                foreach(var state in states){
                    propagator.SetNodeState(state.Key,state.Value);
                }
                checkStates(propagator);
                for(int i = 0;i<50;i++)
                    propagator.Propagate();
                checkStates(propagator);
            }
        }

        [Fact]
        public void SetPosition_SetGraph_Works()
        {
            foreach (var factory in _propagatorFactories)
            {
                var visitedNodes = new List<INode>();
                var visitor = new ActionVisitor<Node,Edge>(
                    (node) =>
                    {
                        lock (visitedNodes)
                            visitedNodes.Add(node);
                    },
                    (edge) => true,
                    () => visitedNodes.Sort());
                var propagator = factory(visitor);
                propagator.SetGraph(_graph);
                propagator.SetPosition(1, 2);
                propagator.Propagate();

                Assert.Equal(visitedNodes, new[] { _graph.Nodes[1], _graph.Nodes[2] });
                visitedNodes.Clear();
                propagator.SetPosition(5, 6);

                propagator.Propagate();
                Assert.Equal(visitedNodes, new[] { _graph.Nodes[5], _graph.Nodes[6] });
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
                var visitor = new ActionVisitor<Node,Edge>(
                    n => visited.Add(n),
                    e => e.TargetId % 2 == 0);
                var propagator = factory(visitor);
                propagator.SetGraph(_graph);
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
            var randNodeId = () => rand.Next(_graph.Nodes.Count());
            foreach (var factory in _propagatorFactories)
            {

                var visitor = new ActionVisitor<Node,Edge>(
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
                propagator.SetGraph(_graph);
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
            var randNode = () => _graph.Nodes[rand.Next(_graph.Nodes.Count())];
            foreach (var factory in _propagatorFactories)
            {

                var visitor = new ActionVisitor<Node,Edge>(
                    n =>
                    {
                        lock (visited)
                            visited.Add(n);
                    },
                    e =>
                    {
                        lock (expected)
                            foreach (var n in _graph.Edges.OutEdges(e.TargetId))
                                expected.Add(_graph.Nodes[n.TargetId]);
                        return true;
                    });
                var propagator = factory(visitor);
                propagator.SetGraph(_graph);
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
                            visitor.Select(new Edge(new Node(-1),e as Node));
                    }
                    visited.Sort();
                    Assert.Equal(buf.Count,visited.Count);
                    Assert.Equal(buf.Select(x => x.Id), visited.Select(x => x.Id));
                    visited.Clear();
                }
            }
        }
        [Fact]
        public void Propagate_HaveRightNodesVisitOrderWithManualData()
        {
            var graph = new Graph<Node, Edge>(new TestGraphConfiguration(new()))
                .Create(10);
            foreach (var pair in ManualTestData.NodesConnections)
            {
                graph.Edges.Add(new Edge(graph.Nodes[pair[0]],graph.Nodes[pair[1]]));
            }

            var actualValues = new List<int>();
            var visitor = new ActionVisitor<Node,Edge>(
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
                    propagator.SetGraph(graph);
                    propagator.SetPosition(expectedValues[0]);

                    for (int i = 0; i < expectedValues.Length; i++)
                    {
                        actualValues.Clear();
                        propagator.Propagate();
                        actualValues.Sort();
                        Assert.Equal(expectedValues[i],actualValues);
                    }
                }
        }
    }
}