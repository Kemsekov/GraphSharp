using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GraphSharp.Helpers
{
    public static class Helpers
    {
        /// <summary>
        /// Search for first N min elements in enumeration using <see cref="Comparison{}"/>
        /// </summary>
        /// <param name="n">Count if elements to search</param>
        /// <param name="src">Source of elements</param>
        /// <param name="comparison">Method to compare two elements</param>
        /// <param name="skipElement">Method to skip some elements</param>
        /// <typeparam name="T">Element type</typeparam>
        /// <returns>First N min elements that was not skipped</returns>
        public static IEnumerable<T> FindFirstNMinimalElements<T>(int n, IEnumerable<T> src,Comparison<T> comparison,Func<T,bool> skipElement = null)
        where T : unmanaged
        {
            if(n<=0) return Enumerable.Empty<T>();
            skipElement ??= (_)=>false;
            var buffer = new T[n];
            int size = 0;
            //front elements is smaller that back elements
            foreach (var el in src)
            {
                if(skipElement(el)) continue;
                if (size!=n)
                {
                    buffer[size++] = el;
                    continue;
                }
                Array.Sort(buffer,comparison);

                if (comparison(el,buffer[^1])<0)
                {
                    Buffer.BlockCopy(buffer,0,buffer,1*Unsafe.SizeOf<T>(),(size-1)*Unsafe.SizeOf<T>());
                    buffer[0] = el;
                }
            }
            Array.Sort(buffer,comparison);
            return buffer;
        }
    }
}