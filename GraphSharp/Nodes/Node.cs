using System.Drawing;
using System.Numerics;
using GraphSharp.Common;
namespace GraphSharp;

/// <summary>
/// Default <see cref="INode"/> implementation.
/// </summary>
public class Node : INode
{
    public static Color DefaultColor = Color.Brown;
    public int Id { get; set; }
    public Vector2 Position { get; set; }
    public Color Color { get; set; } = DefaultColor;
    public float Weight { get; set; }

    public Node(int id)
    {
        Id = id;
    }
    public override string ToString()
    {
        return $"Node {Id}";
    }

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
}