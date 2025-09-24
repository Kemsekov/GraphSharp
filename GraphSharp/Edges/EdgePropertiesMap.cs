using System.Collections.Generic;
using System.Drawing;
using GraphSharp.Extensions;
using MathNet.Numerics.LinearAlgebra.Double;
namespace GraphSharp;

/// <summary>
/// Represents default readings to properties of node
/// </summary>
public partial class EdgePropertiesMap : IEdge
{
    ///<inheritdoc/>
    public object this[string propertyName] { get => Properties[propertyName]; set => Properties[propertyName]=value; }
    /// <summary>
    /// Base edge
    /// </summary>
    public IEdge BaseEdge { get; }
    /// <summary>
    /// Edge cost
    /// </summary>
    public double Cost
    {
        get
        {
            lock (Properties)
            {
                var c = Properties.GetOrDefault("cost");
                if (c is double cap)
                    return cap;
                return 0;
            }
        }
        set
        {
            Properties["cost"] = value;
        }
    }
    /// <summary>
    /// Edge capacity
    /// </summary>
    public double Capacity
    {
        get
        {
            lock (Properties)
            {
                var c = Properties.GetOrDefault("capacity");
                if (c is double cap)
                    return cap;
                return 0;
            }
        }
        set
        {
            lock (Properties)
            Properties["capacity"] = value;
        }
    }
    /// <summary>
    /// Node color
    /// </summary>
    public Color Color
    {
        get
        {
            lock (Properties)
            {
                var c = Properties.GetOrDefault("color");
                if (c is Color color)
                    return color;
                return Color.Empty;
            }
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
            lock (Properties)
            {
                var c = Properties.GetOrDefault("weight");
                if (c is double w)
                    return w;
                return 0;
            }
        }
        set
        {
            lock (Properties)
            Properties["weight"] = value;
        }
    }

    ///<inheritdoc/>
    public IDictionary<string, object> Properties => BaseEdge.Properties;

    ///<inheritdoc/>
    public int SourceId { get => BaseEdge.SourceId; set => BaseEdge.SourceId = value; }
    ///<inheritdoc/>
    public int TargetId { get => BaseEdge.TargetId; set => BaseEdge.TargetId = value; }


    ///<inheritdoc/>
    public EdgePropertiesMap(IEdge edge)
    {
        BaseEdge = edge;
    }

    ///<inheritdoc/>
    public IEdge Clone()
    {
        return BaseEdge.Clone();
    }
}
