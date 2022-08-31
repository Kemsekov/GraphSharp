using System;
namespace GraphSharp.Common;

public interface ICloneable<out T> : ICloneable
where T : ICloneable<T>
{
    new T Clone();
    object ICloneable.Clone() => Clone();
}
