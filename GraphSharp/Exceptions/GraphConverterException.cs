using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Exceptions
{
    public class GraphConverterException : Exception
    {
        public GraphConverterException(string message) : base(message)
        {
        }
    }
}