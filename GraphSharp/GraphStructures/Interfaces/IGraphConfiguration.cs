using System;
using System.Drawing;
using GraphSharp.Edges;
using GraphSharp.Nodes;
namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// A set of methods and properties that used to describe manipulations on nodes and edges
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    public interface IGraphConfiguration<TNode, TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        /// <summary>
        /// <see cref="Random"/> that used to implement's any logic when it reqires random values
        /// </summary>
        public Random Rand {get;}
        IEdgeSource<TNode,TEdge> CreateEdgeSource();
        INodeSource<TNode> CreateNodeSource();
        /// <summary>
        /// Method that used to create instance of <see cref="TNode"/> from it's <see cref="INode.Id"/> as argument
        /// </summary>
        TNode CreateNode(int nodeId);
        /// <summary>
        /// Method that used to create new <see cref="TEdge"/> from two <see cref="TNode"/>
        /// </summary>
        TEdge CreateEdge(TNode source, TNode target);
        /// <summary>
        /// Method that used to calculate distances between nodes.
        /// By default uses their position distances.
        /// </summary>
        /// <returns>Distance between two nodes as a float value</returns>
        float Distance(TNode n1, TNode n2) => (n1.Position-n2.Position).Length();
    }
}