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
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        public Random Rand { get;set; }
        public GraphConfiguration(Random rand)
        {
            Rand = rand;
        }
        public abstract TEdge CreateEdge(TNode source, TNode target);
        public abstract TNode CreateNode(int nodeId);
        public float Distance(TNode n1, TNode n2)
        {
            return (n1.Position-n2.Position).Length();
        }
        public IEdgeSource<TNode,TEdge> CreateEdgeSource()
        {
            return new DefaultEdgeSource<TNode,TEdge>();
        }

        public INodeSource<TNode> CreateNodeSource()
        {
            return new DefaultNodeSource<TNode>(0);
        }
    }
}