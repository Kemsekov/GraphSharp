using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Linq
{
    //taken from https://stackoverflow.com/a/5037815/14586554
    public static class SortExtensions
    {
        //  Sorts an IList<T> in place.
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            ArrayList.Adapter((IList)list).Sort(new ComparisonComparer<T>(comparison));
        }

        // Sorts in IList<T> in place, when T is IComparable<T>
        public static void Sort<T>(this IList<T> list) where T : IComparable<T>
        {
            Comparison<T> comparison = (l, r) => l.CompareTo(r);
            Sort(list, comparison);

        }

        // Convenience method on IEnumerable<T> to allow passing of a
        // Comparison<T> delegate to the OrderBy method.
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> list, Comparison<T> comparison)
        {
            return list.OrderBy(t => t, new ComparisonComparer<T>(comparison));
        }
    }

    // Wraps a generic Comparison<T> delegate in an IComparer to make it easy
    // to use a lambda expression for methods that take an IComparer or IComparer<T>
    public class ComparisonComparer<T> : IComparer<T>, IComparer
    {
        private readonly Comparison<T> _comparison;

        public ComparisonComparer(Comparison<T> comparison)
        {
            _comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return _comparison(x, y);
        }

        public int Compare(object o1, object o2)
        {
            return _comparison((T)o1, (T)o2);
        }
    }
}