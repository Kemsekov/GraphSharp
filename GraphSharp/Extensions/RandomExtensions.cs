using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    public static class RandomExtensions
    {
        /// <returns>
        /// Random integer that satisfies min<=result<=max 
        /// </returns>
        public static int Next(this Random rand,int min,int max){
            return rand.Next(max-min+1)+min;
        }
    }
}