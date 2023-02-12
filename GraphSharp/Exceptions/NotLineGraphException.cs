using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace GraphSharp.Exceptions;

/// <summary>
/// Exception that occurs when some graph needs to be line graph for execution of algorithm,
/// but given graph is not a line graph
/// </summary>
public class NotLineGraphException : Exception
{
    /// <summary>
    /// </summary>
    public NotLineGraphException() : base("Given graph is not a line graph")
    {
    }
    /// <summary>
    /// </summary>
    public NotLineGraphException(string msg) : base(msg){}
}