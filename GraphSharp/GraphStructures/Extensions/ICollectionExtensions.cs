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
}