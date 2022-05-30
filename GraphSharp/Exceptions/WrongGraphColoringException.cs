using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Exceptions
{
    public class WrongGraphColoringException : Exception
    {
        public WrongGraphColoringException(string message) : base(message)
        {
            
        }
    }
}