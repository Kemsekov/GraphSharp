using System;
using GraphSharp.Common;

namespace GraphSharp.Nodes
{
    public interface INode : IColored, IWeighted, IPositioned
    {
        /// <summary>
        /// Unique identifier for node
        /// </summary>
        int Id{get;}

    }
}