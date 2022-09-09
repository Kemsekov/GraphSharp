using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GraphSharp;
/// <summary>
/// Object that holds <paramref name="Array"/> from 
/// <see cref="ArrayPool{}"/> and returns it back to pool when disposed.<br/>
/// Will be disposed by <paramref name="GC"/> if not disposed manually. <br/>
/// </summary>
public class RentedArray<T> : IDisposable, IEnumerable<T>
{
    public int Length { get; }
    public T this[int index]{
        get => index<Length ? array[index] : throw new IndexOutOfRangeException();
        set{
            if(index<Length) array[index] = value;
            else throw new IndexOutOfRangeException();
        }
    }
    ArrayPool<T> pool;
    bool returned = false;
    readonly T[] array;
    public RentedArray(T[] array, int length, ArrayPool<T> pool)
    {
        Array.Fill(array,default);
        this.Length = length;
        this.array = array;
        this.pool = pool;
    }
    ~RentedArray(){
        Dispose();
    }
    public void Fill(T value) => Array.Fill(array,value);    

    public void Dispose()
    {
        if(returned) return;
        pool.Return(array);
        returned = true;
    }
    public ref T At(int index){
        if(index>=Length) throw new IndexOutOfRangeException();
        return ref array[index];
    }

    public IEnumerator<T> GetEnumerator()
    {
        return array.AsEnumerable<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return array.GetEnumerator();
    }
}