using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GraphSharp;
/// <summary>
/// Object that holds byte <paramref name="Array"/> from byte
/// <see cref="ArrayPool{}"/> and returns it back to pool when disposed.<br/>
/// Will be disposed by <paramref name="GC"/> (in destructor) if not disposed manually. <br/>
/// Treats given byte array as storage for any unmanaged type you need
/// </summary>
public unsafe class RentedArray<T> : IDisposable, IEnumerable<T>
where T : unmanaged
{
    /// <summary>
    /// How much elements T can be stored here
    /// </summary>
    public int Length { get; }
    public T this[int index]{
        get => index<Length ? arrayPtr[index] : throw new IndexOutOfRangeException();
        set{
            if(index<Length) arrayPtr[index] = value;
            else throw new IndexOutOfRangeException();
        }
    }
    ArrayPool<byte> pool;
    bool returned = false;
    GCHandle handle;
    T* arrayPtr;
    readonly byte[] array;

    /// <summary>
    /// Creates new instance of <see cref="RentedArray"/>
    /// </summary>
    /// <param name="length">Length of rented array. How many elements of type <paramref name="T"/> need to be stored.</param>
    /// <param name="pool">Pool which will be used to create and return array</param>
    public RentedArray(int length, ArrayPool<byte> pool)
    {
        //Here we calc required size for array to be rented
        int size = Unsafe.SizeOf<T>();
        this.array = pool.Rent(size*length);
        //after we rented an array we need to pin it so GC won't move
        //it's position in VRAM
        this.handle = GCHandle.Alloc(array,GCHandleType.Pinned);
        //after it pinned we can easily take a pointer to array first element
        //and index it like we do it in C/C++
        this.arrayPtr = (T*)Unsafe.AsPointer(ref array[0]);
        //because array pool contains generally random arrays
        //it means we will get array with sufficient size but it will be filled with
        //random stuff, so here we clear our work area with default value
        Array.Fill(array,default);
        this.Length = length;
        this.pool = pool;
    }
    ~RentedArray(){
        Dispose();
    }
    public void Dispose()
    {
        if(returned) return;
        pool.Return(array);
        //don't forget to free pinned object so GC can move it a bit,
        //do memory defragmentation and so on...
        handle.Free();
        returned = true;
    }
    /// <summary>
    /// Fills whole array with given value
    /// </summary>
    public void Fill(T value){
        for(int i = 0;i<Length;i++)
            arrayPtr[i] = value;
    }
    /// <summary>
    /// Method to return element at some index by <see langword="ref"/>
    /// </summary>
    public ref T At(int index){
        if(index>=Length || index<0) throw new IndexOutOfRangeException();
        return ref arrayPtr[index];
    }
    ref T UnsafeAt(int index) => ref arrayPtr[index];

    public IEnumerator<T> GetEnumerator()
    {
        for(int i = 0;i<Length;i++)
            yield return UnsafeAt(i);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return array.GetEnumerator();
    }
}