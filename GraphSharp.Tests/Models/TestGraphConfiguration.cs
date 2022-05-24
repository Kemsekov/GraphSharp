using System;
using System.Drawing;
using GraphSharp.GraphStructures;

namespace GraphSharp.Tests.Models
{
    public class TestGraphConfiguration : GraphConfiguration<TestNode, TestEdge>
    {
        public TestGraphConfiguration(Random rand = null) : base(rand)
        {
            Rand ??= new Random();
        }
        public override TestEdge CreateEdge(TestNode source, TestNode target)
        {
            return new TestEdge(source,target);
        }
        public override TestNode CreateNode(int nodeId)
        {
            return new TestNode(nodeId);
        }
    }
}