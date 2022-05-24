using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Exceptions
{
    public class GraphDataIntegrityException : Exception
    {
        public GraphDataIntegrityException(string message) : base(message)
        {
        }
    }
}