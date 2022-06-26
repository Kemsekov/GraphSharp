using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;

namespace GraphSharp.Common
{
    public static class IGraphConfigurationExtensions
    {
        public static TEdge CloneEdge<TNode, TEdge>(this IGraphConfiguration<TNode, TEdge> configuration, TEdge edge, INodeSource<TNode> nodeSource)
        where TNode : INode
        where TEdge : IEdge<TNode>
        {
            var source = nodeSource[edge.Source.Id];
            var target = nodeSource[edge.Target.Id];
            var weight = edge.Weight;
            var color = edge.Color;

            var newEdge = configuration.CreateEdge(source, target);
            newEdge.Weight = weight;
            newEdge.Color=color;
            return newEdge;
        }
        
        public static TNode CloneNode<TNode, TEdge>(this IGraphConfiguration<TNode, TEdge> configuration, TNode node, Func<TNode,int>? newIndex = null)
        where TNode : INode
        where TEdge : IEdge<TNode>
        {
            newIndex ??= (oldNode)=>oldNode.Id;
            var id = node.Id;
            var color = node.Color;
            var weight = node.Weight;
            var pos = node.Position;

            var newNode = configuration.CreateNode(newIndex(node));
            node.Weight = weight;
            node.Color = color;
            node.Position = pos;
            return newNode;
        }
    }
}