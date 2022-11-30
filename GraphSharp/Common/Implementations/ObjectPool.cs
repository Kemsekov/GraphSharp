using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GraphSharp.Common;
/// <summary>
/// Objects pool that helps to store many objects and reuse them when need
/// </summary>
public class ObjectPool<T>
{
    private readonly ConcurrentBag<T> _objects;
    private readonly Func<T> _objectGenerator;
    /// <summary>
    /// Creates a new instance of object pool
    /// </summary>
    /// <param name="objectGenerator">Function to create object</param>
    public ObjectPool(Func<T> objectGenerator)
    {
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        _objects = new ConcurrentBag<T>();
    }
    /// <summary>
    /// Retrieves a object from pool
    /// </summary>
    public T Get() => _objects.TryTake(out T? item) ? item : _objectGenerator();
    /// <summary>
    /// Returns a back to pool
    /// </summary>
    public void Return(T item) => _objects.Add(item);
    /// <summary>
    /// Returns many objects back to pool
    /// </summary>
    public void Return(IEnumerable<T> items){
        foreach(var i in items) 
            Return(i);
    }
}