using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GraphSharp.Tests.helpers
{
    public class NodesComparer : IComparer<INode>
    {
        public int Compare(INode x, INode y)
        {
            return x.Id-y.Id;
        }
    }
}