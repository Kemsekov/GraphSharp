using System;

namespace GraphSharp.Common
{
    public interface ICloneable<T> : ICloneable
            where T : ICloneable<T>
    {
        new T Clone();
        object ICloneable.Clone()=>Clone();
    }

}