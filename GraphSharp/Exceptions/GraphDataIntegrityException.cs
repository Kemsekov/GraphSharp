using System;
namespace GraphSharp.Exceptions;

public class GraphDataIntegrityException : Exception
{
    public GraphDataIntegrityException(string message) : base(message)
    {
    }
}