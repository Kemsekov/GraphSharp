using System.Collections.Generic;
namespace GraphSharp.Exceptions;

/// <summary>
/// Exception that occurs when requested edge is not found
/// </summary>
public class EdgeNotFoundException : KeyNotFoundException
{
    ///<inheritdoc/>
    public EdgeNotFoundException(string message) : base(message)
    {
    }
}