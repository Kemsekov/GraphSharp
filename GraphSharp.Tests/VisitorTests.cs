using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Tests.Models;
using GraphSharp.Visitors;
using Xunit;

namespace GraphSharp.Tests
{
    public class VisitorTests
    {
        [Fact]
        public void GenericIVisitor_DoNotPassWrongTypes()
        {
            bool visited = false;
            bool selected = false;
            var genericVisitor = new ActionVisitor<TestNode, TestEdge>
            (
                (node) => visited = true,
                (edge) => selected = true,
                () => { }
            );
            var visitor = genericVisitor as IVisitor;
            var node = new Node(5);
            var edge = new Edge(node);

            if (visitor.Select(edge))
                visitor.Visit(node);

            Assert.False(visited);
            Assert.False(selected);

        }
        [Fact]
        public void GenericIVisitor_PassRightTypes()
        {
            bool visited = false;
            bool selected = false;
            var genericVisitor = new ActionVisitor<TestNode, TestEdge>
            (
                (node) => visited = true,
                (edge) => selected = true,
                () => { }
            );
            var visitor = genericVisitor as IVisitor;
            var node = new TestNode();
            var edge = new TestEdge(node);

            if (visitor.Select(edge))
                visitor.Visit(node);

            Assert.True(visited);
            Assert.True(selected);
        }
    }
}