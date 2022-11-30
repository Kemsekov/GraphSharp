using System.Collections.Generic;
namespace GraphSharp.Exceptions;

/// <summary>
/// Occurs required node is not found
/// </summary>
public class NodeNotFoundException : KeyNotFoundException
{
    ///<inheritdoc/>
    public NodeNotFoundException(string message) : base(message)
    {
    }
}