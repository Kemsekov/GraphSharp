using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


namespace GraphSharp.Tests.Helpers
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