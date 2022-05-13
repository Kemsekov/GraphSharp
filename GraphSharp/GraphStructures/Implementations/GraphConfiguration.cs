using System;
using System.Drawing;
using System.Numerics;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Implementation of the <see cref="IGraphConfiguration{,}"/> that uses various interfaces to hide configuration implementation.
    /// </summary>
    public abstract class GraphConfiguration<TNode, TEdge> : IGraphConfiguration<TNode, TEdge>
    where TNode : NodeBase<TEdge>, IWeighted, IColored, IPositioned
    where TEdge : EdgeBase<TNode>, IWeighted, IColored
    {
        public Random Rand { get;set; }
        public GraphConfiguration(Random rand)
        {
            Rand = rand;
        }
        public abstract TEdge CreateEdge(TNode parent, TNode child);
        public abstract TNode CreateNode(int nodeId);
        public float Distance(TNode n1, TNode n2)
        {
            return (n1.Position-n2.Position).Length();
        }
        public Color GetEdgeColor(TEdge edge)
        {
            return edge.Color;
        }
        public float GetEdgeWeight(TEdge edge)
        {
            return edge.Weight;
        }
        public Color GetNodeColor(TNode node)
        {
            return node.Color;
        }
        public Vector2 GetNodePosition(TNode node)
        {
            return node.Position;
        }
        public float GetNodeWeight(TNode node)
        {
            return node.Weight;
        }
        public void SetEdgeColor(TEdge edge, Color color)
        {
            edge.Color = color;
        }
        public void SetEdgeWeight(TEdge edge, float weight)
        {
            edge.Weight = weight;
        }
        public void SetNodeColor(TNode node, Color color)
        {
            node.Color = color;
        }
        public void SetNodePosition(TNode node, Vector2 position)
        {
            node.Position = position;
        }
        public void SetNodeWeight(TNode node, float weight)
        {
            node.Weight = weight;
        }

        public IEdgeSource<TEdge> CreateEdgeSource(int capacity)
        {
            return new DefaultEdgeSource<TEdge>(capacity);
        }

        public INodeSource<TNode> CreateNodeSource(int capacity)
        {
            return new DefaultNodeSource<TNode>(capacity);
        }
    }
}