using GraphSharp.Propagators;

namespace GraphSharp.Common;
/// <summary>
/// Contains all states used by <see cref="PropagatorBase{TEdge}"/>
/// </summary>
public struct UsedNodeStates
{
    /// <summary>
    /// None state for node.
    /// </summary>
    public const byte None = 0;
    /// <summary>
    /// In this state node is in check for visit in next iteration of any algorithm
    /// </summary>
    public const byte ToVisit = 1;
    /// <summary>
    /// In this state node is required to visit for current iteration
    /// </summary>
    public const byte Visited = 2;
    /// <summary>
    /// In this state on each iteration "in edges" of node is chosen as next generation
    /// </summary>
    public const byte IterateByInEdges = 4;
    /// <summary>
    /// In this state on each iteration "out edges" of node is chosen as next generation
    /// </summary>
    public const byte IterateByOutEdges = 8;
}