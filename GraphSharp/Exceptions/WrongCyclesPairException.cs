using System;
namespace GraphSharp.Exceptions;

public class WrongCyclesPairException : Exception
{
    public WrongCyclesPairException(string message) : base(message)
    {
        
    }
}