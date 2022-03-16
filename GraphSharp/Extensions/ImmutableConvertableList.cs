using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Extensions
{
    public class ImmutableConvertableList<T,TBase> : IImmutableConvertableList<TBase>
    where T : TBase
    {
        IList<T> source;
        public ImmutableConvertableList(IList<T> source)
        {
            this.source = source;
        }
        public TBase this[int index] => source[index];
        public int Count => source.Count;


        public IEnumerator<TBase> GetEnumerator()
        {
            int index = 0;
            while(index<source.Count)
                yield return source[index++];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)source).GetEnumerator();
        }
    }
}