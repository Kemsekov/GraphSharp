using System;
using GraphSharp.Common;
namespace GraphSharp;

/// <summary>
/// Interface for all edges.
/// </summary>
public interface IEdge : IComparable<IEdge>, ICloneable<IEdge>, IFlowed, IWeighted, IColored
{
    /// <summary>
    /// Id of a source node of this edge
    /// </summary>
    int SourceId { get; set; }
    /// <summary>
    /// Id of a target node of this edge
    /// </summary>
    int TargetId { get; set; }
    int IComparable<IEdge>.CompareTo(IEdge? other)
    {
        if (other is null)
            return 1;
        var d1 = SourceId - other.SourceId;
        var d2 = TargetId - other.TargetId;
        if (d1 == 0) return d2;
        return d1;
    }
    
}