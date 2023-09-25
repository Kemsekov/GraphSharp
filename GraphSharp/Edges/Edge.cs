using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using GraphSharp.Common;
namespace GraphSharp;



/// <summary>
/// Default <see cref="IEdge"/> implementation
/// </summary>
public class Edge : IEdge
{
    ///<inheritdoc/>
    public static Color DefaultColor = Color.DarkViolet;
    ///<inheritdoc/>
    public int SourceId { get; set; }
    ///<inheritdoc/>
    public int TargetId { get; set; }
    ///<inheritdoc/>
    public double Weight { get; set; }
    ///<inheritdoc/>
    public Color Color { get; set; } = DefaultColor;
    ///<inheritdoc/>
    public IDictionary<string, object> Properties{get;init;}
    /// <summary>
    /// Creates a new instance of edge
    /// </summary>
    public Edge(INode source, INode target) : this(source.Id,target.Id){}
    /// <summary>
    /// Creates a new instance of edge
    /// </summary>
    public Edge(int sourceId, int targetId)
    {
        SourceId = sourceId;
        TargetId = targetId;
        Properties = new Dictionary<string,object>();
    }
    ///<inheritdoc/>
    public override string ToString()
    {
        return $"Edge {SourceId}->{TargetId}";
    }
    ///<inheritdoc/>
    public virtual IEdge Clone()
    {
        return new Edge(SourceId, TargetId)
        {
            Weight = this.Weight,
            Color = this.Color,
        };
    }
}