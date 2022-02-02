using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GraphSharp.Helpers
{
    public static class Helpers
    {
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