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
        where TNode : NodeBase<TEdge>
        where TEdge : EdgeBase<TNode>
        {
            var source = nodeSource[edge.Source.Id];
            var target = nodeSource[edge.Target.Id];
            var weight = configuration.GetEdgeWeight(edge);
            var color = configuration.GetEdgeColor(edge);

            var newEdge = configuration.CreateEdge(source, target);
            configuration.SetEdgeWeight(newEdge, weight);
            configuration.SetEdgeColor(newEdge, color);
            return newEdge;
        }
        
        public static TNode CloneNode<TNode, TEdge>(this IGraphConfiguration<TNode, TEdge> configuration, TNode node, Func<TNode,int>? newIndex = null)
        where TNode : NodeBase<TEdge>
        where TEdge : EdgeBase<TNode>
        {
            newIndex ??= (oldNode)=>oldNode.Id;
            var id = node.Id;
            var color = configuration.GetNodeColor(node);
            var weight = configuration.GetNodeWeight(node);
            var pos = configuration.GetNodePosition(node);

            var newNode = configuration.CreateNode(newIndex(node));
            configuration.SetNodeWeight(node, weight);
            configuration.SetNodeColor(node, color);
            configuration.SetNodePosition(node, pos);
            return newNode;
        }
    }
}