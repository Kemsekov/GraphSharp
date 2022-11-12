using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
public interface IPath<TNode>
{
    IEnumerable<TNode> Path{get;}
    double Cost{get;}
}