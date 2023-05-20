using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
/// <summary>
/// Struct that used to message a movement from one node to another by some edge
/// </summary>
public struct EdgeSelect<TEdge>
where TEdge : IEdge
{
    /// <param name="edge">Edge which used to move</param>
    /// <param name="sourceId">
    /// Source id - nodeId that was a predecessor for calling this edge. 
    /// Other part of the edge is basically a node we trying to move.
    /// </param>
    public EdgeSelect(TEdge edge, int sourceId)
    {
        Edge = edge;
        SourceId = sourceId;
        TargetId = Edge.Other(sourceId);
    }
    /// <summary>
    /// Underlying edge that being selected
    /// </summary>
    public TEdge Edge;
    /// <summary>
    /// Source id
    /// </summary>
    public int SourceId{get;}
    /// <summary>
    /// Target id
    /// </summary>
    public int TargetId{get;}
    /// <summary>
    /// Other part of the edge, or -1 if not found
    /// </summary>
    public int Other(int nodeId) => Edge.Other(nodeId);
    /// <returns>True if edges connect same nodes, without taking their directness into accountants</returns>
    public bool ConnectsSame(IEdge edge){
        return Edge.ConnectsSame(edge);
    }
    /// <summary>
    /// Converts <see langword="TEdge"/> to <see cref="EdgeSelect{T}"/>
    /// </summary>
    public static implicit operator TEdge(EdgeSelect<TEdge> e) => e.Edge;
}