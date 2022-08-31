using System.Collections.Generic;
namespace GraphSharp.Exceptions;

public class NodeNotFoundException : KeyNotFoundException
{
    public NodeNotFoundException(string message) : base(message)
    {
    }
}