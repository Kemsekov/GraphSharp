using System;
namespace GraphSharp.Exceptions;

public class GraphConverterException : Exception
{
    public GraphConverterException(string message) : base(message)
    {
    }
}