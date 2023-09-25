using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.Common;

/// <summary>
/// Contains vector that describes position
/// </summary>
public interface IPositioned{
    /// <summary>
    /// Object position in n-dimensional space
    /// </summary>
    public Vector Position{get;}
}