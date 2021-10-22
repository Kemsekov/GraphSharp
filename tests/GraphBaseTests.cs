
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
        public void AddVisitor_ThrowsIfOutOfRange()
        {
            var graph = new Graph(Enumerable.Range(1, 5).Select(i => new Node(i)));
            var visitor = new ActionVisitor(node => { });
            Assert.Throws<IndexOutOfRangeException>(() => graph.AddVisitor(visitor, 22));
        }
        [Fact]
        public void RemoveVisitor_Works()
        {
            var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node,NodeBase>(1000, 30, 70);
            var graph = new Graph(nodes);

            var childs1 = new List<NodeBase>();
            var childs2 = new List<NodeBase>();


            var visitor1 = new ActionVisitor(node =>
            {
                
                lock (childs1)
                    childs1.Add(node);
            });
            var visitor2 = new ActionVisitor(node =>
            {
                
                lock (childs2)
                    childs2.Add(node);
            });
            graph.AddVisitor(visitor1, 1);
            graph.AddVisitor(visitor2, 2);

            graph.Step();
            childs1.Clear();
            childs2.Clear();

            graph.Step();
            childs1.Sort();
            nodes[1].Children.Sort();
            nodes[2].Children.Sort();

            Assert.Equal(childs1, nodes[1].Children);
            Assert.Equal(childs2.Count, nodes[2].Children.Count);
            Assert.Equal(childs2, nodes[2].Children);

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
            var visitor1 = new ActionVisitor(node => { });
            var visitor2 = new ActionVisitor(node => { });

            graph.AddVisitor(visitor1, 1);

            Assert.Throws<IndexOutOfRangeException>(() =>
                graph.AddVisitor(visitor1, 10));

            Assert.Throws<KeyNotFoundException>(() =>
                graph.Step(visitor2));
        }

    }
}
