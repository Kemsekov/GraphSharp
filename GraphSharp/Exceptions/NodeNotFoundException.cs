using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Exceptions
{
    public class NodeNotFoundException : KeyNotFoundException
    {
        public NodeNotFoundException(string message) : base(message)
        {
        }
    }
}