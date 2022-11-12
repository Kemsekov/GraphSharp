using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
public record PathResult<TNode> : IPath<TNode>
{
    Lazy<double> CostLazy { get; init; }
    public PathResult(Func<IEnumerable<TNode>,double> computePath, IEnumerable<TNode> path)
    {
        Path = path;
        this.CostLazy = new Lazy<double>(()=>computePath(Path));
    }
    public double Cost => CostLazy.Value;
    public IEnumerable<TNode> Path{get;init;}
}