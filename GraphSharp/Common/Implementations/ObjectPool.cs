using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GraphSharp.Common;
public class ObjectPool<T>
{
    private readonly ConcurrentBag<T> _objects;
    private readonly Func<T> _objectGenerator;
    public ObjectPool(Func<T> objectGenerator)
    {
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        _objects = new ConcurrentBag<T>();
    }
    public T Get() => _objects.TryTake(out T? item) ? item : _objectGenerator();
    public void Return(T item) => _objects.Add(item);
    public void Return(IEnumerable<T> items){
        foreach(var i in items) 
            Return(i);
    }
}