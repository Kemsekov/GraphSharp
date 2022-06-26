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
        int IComparable<INode>.CompareTo(INode? other){
            return this.Id-other?.Id ?? throw new NullReferenceException("Can't compare node that null");
        }

    }
}