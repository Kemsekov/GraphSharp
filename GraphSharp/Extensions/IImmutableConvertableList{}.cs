using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Extensions
{
    public interface IImmutableConvertableList<T> : IEnumerable<T>
    {
        T this[int index]{get;}
        int Count{get;}
    }
}