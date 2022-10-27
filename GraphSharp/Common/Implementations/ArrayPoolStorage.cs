using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace GraphSharp;

/// <summary>
/// Storage for array pools of required types and rent class for simpler resource control.
/// </summary>
public static class ArrayPoolStorage
{
    /// <summary>
    /// This is pretty much limitation to how much nodes can be used in this library <br/>
    /// Array pools this class uses have this parameter as limitation to <paramref name="maxArrayLength"/>
    /// </summary>
    public const int MaxArrayLength = 33554432;
    /// <summary>
    /// Parameter to how much similar size arrays will be close to fetch in created array pool.<br/>
    /// </summary>
    public const int MaxArraysPerBucket = 128;
    static readonly ArrayPool<byte> ByteArrayPool = ArrayPool<byte>.Create(MaxArrayLength,MaxArraysPerBucket);
    public static RentedArray<int> RentIntArray(int length){
        return new(length,ByteArrayPool);
    }
    public static RentedArray<uint> RentUintArray(int length){
        return new(length,ByteArrayPool);
    }
    public static RentedArray<float> RentFloatArray(int length){
        return new(length,ByteArrayPool);
    }
    public static RentedArray<byte> RentByteArray(int length){
        return new(length,ByteArrayPool);
    }
    public static RentedArray<UnmanagedColor> RentColorArray(int length){
        return new(length,ByteArrayPool);
    }
    public static RentedArray<T> RentArray<T>(int length)
    where T : unmanaged
    {
        return new(length,ByteArrayPool);
    }
}