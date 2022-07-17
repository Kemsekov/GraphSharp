using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Nodes
{
    public class NodeComparer<TNode> : IComparer<TNode>
    where TNode : INode
    {
        public int Compare(TNode? x, TNode? y)
        {
            if(x is null || y is null) throw new NullReferenceException("Cannot compare nodes that null!");
            return x.Id-y.Id;
        }
    }
}