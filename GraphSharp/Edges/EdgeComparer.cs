using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GraphSharp
{
    public class EdgeComparer<TNode, TEdge> : IComparer<TEdge>
    where TNode : INode
    where TEdge : IEdge
    {
        public int Compare(TEdge? x, TEdge? y)
        {
            if(x is null || y is null) throw new NullReferenceException("Cannot compare null edges!");
            return x.CompareTo(y);
        }
    }
}