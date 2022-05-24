using System;
using System.Drawing;
using System.Numerics;
using GraphSharp.GraphStructures;

public class EmptyGraphConfiguration : IGraphConfiguration<EmptyNode, EmptyEdge>
{
    public Random Rand{get;} = new();

    public EmptyEdge CreateEdge(EmptyNode source, EmptyNode target)
    {
        return new EmptyEdge(source, target);
    }

    public IEdgeSource<EmptyEdge> CreateEdgeSource()
    {
        return new DefaultEdgeSource<EmptyNode,EmptyEdge>();
    }

    public EmptyNode CreateNode(int nodeId)
    {
        return new EmptyNode(nodeId);
    }

    public INodeSource<EmptyNode> CreateNodeSource()
    {
        return new DefaultNodeSource<EmptyNode>(0);
    }
    public float Distance(EmptyNode n1, EmptyNode n2)
    {
        throw new NotImplementedException();
    }

    public Color GetEdgeColor(EmptyEdge edge)
    {
        throw new NotImplementedException();
    }

    public float GetEdgeWeight(EmptyEdge edge)
    {
        throw new NotImplementedException();
    }

    public Color GetNodeColor(EmptyNode node)
    {
        throw new NotImplementedException();
    }

    public Vector2 GetNodePosition(EmptyNode node)
    {
        throw new NotImplementedException();
    }

    public float GetNodeWeight(EmptyNode node)
    {
        throw new NotImplementedException();
    }

    public void SetEdgeColor(EmptyEdge edge, Color color)
    {
        throw new NotImplementedException();
    }

    public void SetEdgeWeight(EmptyEdge edge, float weight)
    {
        throw new NotImplementedException();
    }

    public void SetNodeColor(EmptyNode node, Color color)
    {
        throw new NotImplementedException();
    }

    public void SetNodePosition(EmptyNode node, Vector2 position)
    {
        throw new NotImplementedException();
    }

    public void SetNodeWeight(EmptyNode node, float weight)
    {
        throw new NotImplementedException();
    }
}