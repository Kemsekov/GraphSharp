using System.Collections.Generic;
namespace GraphSharp.Exceptions;

public class EdgeNotFoundException : KeyNotFoundException
{
    public EdgeNotFoundException(string message) : base(message)
    {
    }
}