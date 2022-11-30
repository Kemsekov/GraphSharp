using System;
namespace GraphSharp.Common;

///<inheritdoc/>
public interface ICloneable<out T> : ICloneable
where T : ICloneable<T>
{
    /// <summary>
    /// Clones object
    /// </summary>
    new T Clone();
    object ICloneable.Clone() => Clone();
}
