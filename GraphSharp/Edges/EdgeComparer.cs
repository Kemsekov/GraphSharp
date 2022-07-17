using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.Edges
{
    public class EdgeComparer<TNode, TEdge> : IComparer<TEdge>
    where TNode : INode
    where TEdge : IEdge<TNode>
    {
        public int Compare(TEdge? x, TEdge? y)
        {
            if(x is null || y is null) throw new NullReferenceException("Cannot compare null edges!");
            var d1 = x.Source.Id-y.Source.Id;
            var d2 = x.Target.Id-y.Target.Id;
            return d1==0 ? d2 : d1;
        }
    }
}