using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures.Interfaces
{
    /// <summary>
    /// A set of methods and properties that used to describe manipulations on nodes and edges
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    public interface IGraphConfiguration<TNode, TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        /// <summary>
        /// <see cref="Random"/> that used to implement's any logic when it reqires random values
        /// </summary>
        public Random Rand { get;set; }
        /// <summary>
        /// Method that used to create instance of <see cref="TNode"/> from it's <see cref="INode.Id"/> as argument
        /// </summary>
        TNode CreateNode(int nodeId);
        /// <summary>
        /// Method that used to create new <see cref="TEdge"/> from two <see cref="TNode"/>
        /// </summary>
        TEdge CreateEdge(TNode parent, TNode child);
        /// <summary>
        /// Method that used to determite how to calculate distance between two <see cref="TNode"/>
        /// </summary>
        float Distance(TNode n1, TNode n2);
        /// <summary>
        /// Method that used to get weight from particular <see cref="TEdge"/>
        /// </summary>
        float GetEdgeWeight(TEdge edge);
        /// <summary>
        /// Method that used to assign weight to particular <see cref="TEdge"/>
        /// </summary>
        void SetEdgeWeight(TEdge edge, float weight);
        /// <summary>
        /// Method that used to get weight from particular <see cref="TNode"/>
        /// </summary>
        float GetNodeWeight(TNode node);
        /// <summary>
        /// Method that used to assign weight to particular <see cref="TNode"/>
        /// </summary>
        /// <value></value>
        void SetNodeWeight(TNode node, float weight);
    }
}