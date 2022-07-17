using System;
using GraphSharp.Common;
using GraphSharp.Nodes;
namespace GraphSharp.Edges
{
    public interface IEdge<TNode> : IColored, IWeighted, IFlowed
    {
        TNode Source{get;set;}
        TNode Target{get;set;}
    }
}