using System;
using System.Collections.Generic;
using System.Drawing;
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

        public Color GetEdgeColor(TestEdge edge)
        {
            return edge.Color;
        }

        public float GetEdgeWeight(TestEdge edge)
        {
            return edge.Weight;
        }

        public Color GetNodeColor(TestNode node)
        {
            return node.Color;
        }

        public float GetNodeWeight(TestNode node)
        {
            return node.Weight;
        }

        public void SetEdgeColor(TestEdge edge, Color color)
        {
            edge.Color = color;
        }

        public void SetEdgeWeight(TestEdge edge, float weight)
        {
            edge.Weight = weight;
        }

        public void SetNodeColor(TestNode node, Color color)
        {
            node.Color = color;
        }

        public void SetNodeWeight(TestNode node, float weight)
        {
            node.Weight = weight;
        }
    }
}