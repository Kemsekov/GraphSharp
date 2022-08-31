using System;
namespace GraphSharp.Exceptions;

public class WrongGraphColoringException : Exception
{
    public WrongGraphColoringException(string message) : base(message)
    {

    }
}