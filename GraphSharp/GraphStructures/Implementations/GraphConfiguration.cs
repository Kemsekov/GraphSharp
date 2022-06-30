using System;
using System.Drawing;
using System.Numerics;
using GraphSharp.Common;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Default <see cref="IGraphConfiguration{,}"/> implementation that uses default configuration.
    /// </summary>
    public class GraphConfiguration<TNode, TEdge> : IGraphConfiguration<TNode, TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        private Func<TNode, TNode, TEdge> createEdge;
        private Func<int, TNode> createNode;

        public Random Rand { get;set; }
        public GraphConfiguration(Random rand,Func<TNode,TNode,TEdge> createEdge, Func<int,TNode> createNode)
        {
            this.createEdge = createEdge;
            this.createNode = createNode;
            Rand = rand;
        }
        public TEdge CreateEdge(TNode source, TNode target) => createEdge(source,target);
        public TNode CreateNode(int nodeId) => createNode(nodeId);
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