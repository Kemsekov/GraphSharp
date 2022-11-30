using System;
namespace GraphSharp.Exceptions;

/// <summary>
/// Exception that occurs when graph coloring is invalid(two adjacent edges have same color)
/// </summary>
public class WrongGraphColoringException : Exception
{
    ///<inheritdoc/>
    public WrongGraphColoringException(string message) : base(message)
    {

    }
}