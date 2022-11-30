using System;
namespace GraphSharp.Exceptions;

/// <summary>
/// Occurs when any graph converter failed
/// </summary>
public class GraphConverterException : Exception
{
    ///<inheritdoc/>
    public GraphConverterException(string message) : base(message)
    {
    }
}