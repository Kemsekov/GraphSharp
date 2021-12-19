using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GraphSharp.Nodes;

namespace tests.Helpers
{
    public class NodeEqualityComparer : IEqualityComparer<INode>
    {
        public bool Equals(INode x, INode y)
        {
            return x.Id==y.Id;
        }

        public int GetHashCode([DisallowNull] INode obj)
        {
            return obj.Id;
        }
    }
}