using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Exceptions;
/// <summary>
/// Occurs when node id not equals to index that used to store it.
/// </summary>
public class WrongNodeSetException : Exception
{
    ///<inheritdoc/>
    public WrongNodeSetException(string msg) : base(msg)
    {

    }
}