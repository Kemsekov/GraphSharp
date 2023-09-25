using System.Collections.Generic;
using System.Drawing;
using GraphSharp.Extensions;
using MathNet.Numerics.LinearAlgebra.Single;
namespace GraphSharp;

/// <summary>
/// Represents default readings to properties of node
/// </summary>
public partial class NodePropertiesMap : INode
{
    /// <summary>
    /// Base node
    /// </summary>
    public INode BaseNode { get; }
    /// <summary>
    /// Node color
    /// </summary>
    public Color Color
    {
        get
        {
            var c = Properties.GetOrDefault("color");
            if (c is Color color)
                return color;
            return Color.Empty;
        }
        set
        {
            Properties["color"] = value;
        }
    }
    /// <summary>
    /// Node weight
    /// </summary>
    public double Weight
    {
        get
        {
            var c = Properties.GetOrDefault("weight");
            if (c is double w)
                return w;
            return 0;
        }
        set
        {
            Properties["weight"] = value;
        }
    }
    ///<inheritdoc/>
    public Vector Position{
        get
        {
            var c = Properties.GetOrDefault("position");
            if (c is Vector w)
                return w;
            var newW =  DenseVector.Create(2,0);
            Properties["position"] = newW;
            return newW;
        }
        set
        {
            Properties["position"] = value;
        }
    }
    ///<inheritdoc/>
    public IDictionary<string, object> Properties => BaseNode.Properties;

    ///<inheritdoc/>
    public int Id { get => BaseNode.Id; set => BaseNode.Id = value; }

    ///<inheritdoc/>
    public NodePropertiesMap(INode node)
    {
        BaseNode = node;
    }

    ///<inheritdoc/>
    public INode Clone()
    {
        return BaseNode.Clone();
    }

    ///<inheritdoc/>
    public bool Equals(INode? other)
    {
        return BaseNode.Equals(other);
    }
}
