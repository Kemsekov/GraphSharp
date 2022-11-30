using System;
namespace GraphSharp.Exceptions;

/// <summary>
/// Occurs when you feed two linear-dependent cycles in cycle combiner 
/// </summary>
public class WrongCyclesPairException : Exception
{
    ///<inheritdoc/>
    public WrongCyclesPairException(string message) : base(message)
    {
        
    }
}