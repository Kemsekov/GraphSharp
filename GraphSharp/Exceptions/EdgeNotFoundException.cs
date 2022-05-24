using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Exceptions
{
    public class EdgeNotFoundException : KeyNotFoundException
    {
        public EdgeNotFoundException(string message) : base(message)
        {
        }
    }
}