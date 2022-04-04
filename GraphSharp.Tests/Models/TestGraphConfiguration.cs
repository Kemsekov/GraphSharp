using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.GraphStructures.Interfaces;

namespace GraphSharp.Tests.Models
{
    public class TestGraphConfiguration : IGraphConfiguration<TestNode, TestEdge>
    {
        public Random Rand{get;set;} = new();

        public TestEdge CreateEdge(TestNode parent, TestNode child)
        {
            return new TestEdge(parent,child);
        }
        

        public TestNode CreateNode(int nodeId)
        {
            return new TestNode(nodeId);
        }

        public float Distance(TestNode n1, TestNode n2)
        {
            return n1.Id-n2.Id;
        }

        public float GetEdgeWeight(TestEdge edge)
        {
            return edge.Weight;
        }

        public float GetNodeWeight(TestNode node)
        {
            return node.Weight;
        }

        public void SetEdgeWeight(TestEdge edge, float weight)
        {
            edge.Weight = weight;
        }

        public void SetNodeWeight(TestNode node, float weight)
        {
            node.Weight = weight;
        }
    }
}