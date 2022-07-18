using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Exceptions
{
    public class WrongCyclesPairException : Exception
    {
        public WrongCyclesPairException(string message) : base(message)
        {
            
        }
    }
}