using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Extensions
{
    /// <summary>
    /// Allows treat IEnumerable{T} as IEnumerable{TBase}
    /// </summary>
    /// <typeparam name="T">Type that inhereted from TBase</typeparam>
    /// <typeparam name="TBase">Base type for T</typeparam>
    public class ConvertableEnumerable<T, TBase> : IEnumerable<TBase>
    where T : TBase
    {
        IEnumerable<T> list;
        public ConvertableEnumerable(IEnumerable<T> src)
        {
            list = src;
        }

        public IEnumerator<TBase> GetEnumerator()
        {
            using IEnumerator<T> ie = list.GetEnumerator();
            while (ie.MoveNext())
            {
                yield return ie.Current;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }
        
    }
}