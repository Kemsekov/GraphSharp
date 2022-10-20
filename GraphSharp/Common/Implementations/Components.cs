using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
public class Components<TNode> : IDisposable
{
    public Components(IEnumerable<TNode>[] components, UnionFind setFinder)
    {
        this.Components_ = components;
        this.SetFinder = setFinder;
    }
    public IEnumerable<TNode>[] Components_ { get; }
    public UnionFind SetFinder { get; }

    public void Dispose()
    {
        SetFinder.Dispose();
    }
    public bool InSameComponent(int nodeId1,int nodeId2){
        return SetFinder.FindSet(nodeId1)==SetFinder.FindSet(nodeId2);
    }
}