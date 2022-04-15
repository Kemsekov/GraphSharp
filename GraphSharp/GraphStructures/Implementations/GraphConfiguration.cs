using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures.Interfaces;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public class GraphConfiguration<TNode, TEdge> : IGraphConfiguration<TNode, TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public Random Rand { get;set; }
        public Func<TNode, TNode, TEdge> CreateEdgeImpl{get;set;}
        public Func<int, TNode> CreateNodeImpl{get;set;}
        public Func<TNode, TNode, float> DistanceImpl{get;set;}
        public Func<TEdge, float> GetEdgeWeightImpl{get;set;}
        public Func<TNode, float> GetNodeWeightImpl{get;set;}
        public Action<TEdge, float> SetEdgeWeightImpl{get;set;}
        public Action<TNode, float> SetNodeWeightImpl{get;set;}
        public Func<TNode,Color> GetNodeColorImpl{get;set;}
        public Action<TNode,Color> SetNodeColorImpl{get;set;}
        public Func<TEdge, Color> GetEdgeColorImpl{get;set;}
        public Action<TEdge, Color> SetEdgeColorImpl{get;set;}
        public GraphConfiguration(
            Func<int,TNode> createNode, 
            Func<TNode,TNode,TEdge> createEdge,
            Func<TNode,TNode,float> distance, 
            Func<TNode, float> getNodeWeight,
            Action<TNode,float> setNodeWeight,
            Func<TEdge,float> getEdgeWeight,
            Action<TEdge,float> setEdgeWeight,
            Func<TNode,Color> getNodeColor,
            Action<TNode,Color> setNodeColor,
            Func<TEdge, Color> getEdgeColor,
            Action<TEdge, Color> setEdgeColor,
            Random rand = null
            )
        {
            CreateEdgeImpl = createEdge;
            CreateNodeImpl = createNode;
            DistanceImpl = distance;
            GetEdgeWeightImpl = getEdgeWeight;
            GetNodeWeightImpl = getNodeWeight;
            SetEdgeWeightImpl = setEdgeWeight;
            SetNodeWeightImpl = setNodeWeight;
            GetNodeColorImpl = getNodeColor;
            SetNodeColorImpl = setNodeColor;
            GetEdgeColorImpl = getEdgeColor;
            SetEdgeColorImpl = setEdgeColor;
            Rand = rand ?? new Random();
        }
        public TEdge CreateEdge(TNode parent, TNode child)
        {
            return CreateEdgeImpl(parent,child);
        }

        public TNode CreateNode(int nodeId)
        {
            return CreateNodeImpl(nodeId);
        }

        public float Distance(TNode n1, TNode n2)
        {
            return DistanceImpl(n1,n2);
        }

        public float GetEdgeWeight(TEdge edge)
        {
            return GetEdgeWeightImpl(edge);
        }

        public float GetNodeWeight(TNode node)
        {
            return GetNodeWeightImpl(node);
        }

        public void SetEdgeWeight(TEdge edge, float weight)
        {
            SetEdgeWeightImpl(edge,weight);
        }

        public void SetNodeWeight(TNode node, float weight)
        {
            SetNodeWeightImpl(node,weight);
        }

        public Color GetNodeColor(TNode node)
        {
            return GetNodeColorImpl(node);
        }

        public void SetNodeColor(TNode node, Color color)
        {
            SetNodeColorImpl(node,color);
        }

        public Color GetEdgeColor(TEdge edge)
        {
            return GetEdgeColorImpl(edge);
        }

        public void SetEdgeColor(TEdge edge, Color color)
        {
            SetEdgeColorImpl(edge,color);
        }
    }
}