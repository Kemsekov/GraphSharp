using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Graphs;
public static class ICollectionExtensions
{
    /// <summary>
    /// Removes all elements that satisfies given predicate
    /// </summary>
    /// <returns>Count of elements removed</returns>
    public static int RemoveAll<T>(this ICollection<T> collection, Predicate<T> toRemove)
    {
        if (collection is List<T> list)
        {
            return list.RemoveAll(toRemove);
        }
        List<T> itemsToDelete = collection
            .Where(x => toRemove(x))
            .ToList();

        foreach (var item in itemsToDelete)
        {
            collection.Remove(item);
        }
        return itemsToDelete.Count;
    }
    // TODO: add test
    /// <returns>
    /// All values that share the same minimal score
    /// </returns>
    public static IList<T> AllMinValues<T>(this IEnumerable<T> e, Func<T, float> getMeasure)
    {
        var result = new List<T>();
        var bestScore = float.MaxValue;
        foreach(var n in e)
        {
            var score = getMeasure(n);

            if (score < bestScore)
            {
                bestScore = score;
                result.Clear();
            }

            if (score == bestScore)
            {
                result.Add(n);
            }
        };
        return result;
    }
}