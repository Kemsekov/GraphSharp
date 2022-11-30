using System;
namespace GraphSharp.Exceptions;

/// <summary>
/// Occurs when there is inconsistency in nodes and / or edges in a graph.<br/>
/// When graph's internal structure is invalid.
/// </summary>
public class GraphDataIntegrityException : Exception
{
    ///<inheritdoc/>
    public GraphDataIntegrityException(string message) : base(message)
    {
    }
}