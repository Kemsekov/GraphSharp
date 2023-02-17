using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Single;
using GraphSharp.Common;
namespace GraphSharp;

/// <summary>
/// Default <see cref="INode"/> implementation.
/// </summary>
public class Node : INode
{
    ///<inheritdoc/>
    public static Color DefaultColor = Color.Brown;
    ///<inheritdoc/>
    public int Id { get; set; }
    ///<inheritdoc/>
    public Vector Position { get; set; }
    ///<inheritdoc/>
    public Color Color { get; set; } = DefaultColor;
    ///<inheritdoc/>
    public double Weight { get; set; }

    ///<inheritdoc/>
    public Node(int id)
    {
        Id = id;
        Position = new DenseVector(new float[0]);
    }
    ///<inheritdoc/>
    public override string ToString()
    {
        return $"Node {Id}";
    }

    ///<inheritdoc/>
    public Node Clone()
    {
        return new Node(Id)
        {
            Weight = this.Weight,
            Position = this.Position,
            Color = this.Color
        };
    }

    INode ICloneable<INode>.Clone() => Clone();

    ///<inheritdoc/>
    public bool Equals(INode? other)
    {
        return other?.Id == Id;
    }
}