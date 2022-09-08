using System.Buffers;
namespace GraphSharp.Common;

/// <summary>
/// Storage for array pools of required types
/// </summary>
public static class ArrayPoolStorage
{
    public static readonly ArrayPool<int> IntArrayPool = ArrayPool<int>.Create();
    public static readonly ArrayPool<float> FloatArrayPool = ArrayPool<float>.Create();
    public static readonly ArrayPool<byte> ByteArrayPool = ArrayPool<byte>.Create();
}