using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Single;
using GraphSharp.Common;
using System.Collections.Generic;
using System.Collections.Concurrent;
using GraphSharp.Extensions;
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
    public IDictionary<string, object> Properties{get;init;}
    ///<inheritdoc/>
    public Node(int id)
    {
        Id = id;
        Properties = new Dictionary<string,object>();
        Properties["color"] = DefaultColor;
        Properties["position"] = DenseVector.Create(2,0);
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
            Properties=this.Properties.Clone()
        };
    }

    INode ICloneable<INode>.Clone() => Clone();

    ///<inheritdoc/>
    public bool Equals(INode? other)
    {
        return other?.Id == Id;
    }
}