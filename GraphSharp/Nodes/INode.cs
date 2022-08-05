using System;
using GraphSharp.Common;

namespace GraphSharp
{
    public interface INode : IComparable<INode>, ICloneable<INode>, IPositioned, IColored, IWeighted
    {
        /// <summary>
        /// Unique identifier for node
        /// </summary>
        int Id{get;set;}
        int IComparable<INode>.CompareTo(INode? other)
        {
            other = other ?? throw new NullReferenceException("Cannot compare node that is null");
            return Id - other.Id;
        }
    }
}