using System;
using GraphSharp.Common;

namespace GraphSharp.Nodes
{
    public interface INode : IComparable<INode>, IColored, IWeighted, IPositioned
    {
        /// <summary>
        /// Unique identifier for node
        /// </summary>
        int Id{get;}
        int  IComparable<INode>.CompareTo(INode? other)
        {
            other = other ?? throw new NullReferenceException("Cannot compare node that is null");
            return Id - other.Id;
        }
    }
}