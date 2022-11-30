using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common;
/// <summary>
/// Result of components finder algorithms
/// </summary>
/// <typeparam name="TNode"></typeparam>
public class ComponentsResult<TNode> : IDisposable
{
    ///<inheritdoc/>
    public ComponentsResult(IEnumerable<TNode>[] components, UnionFind setFinder)
    {
        this.Components = components;
        this.SetFinder = setFinder;
    }
    /// <summary>
    /// A list of components, where each element is a array of nodes that represents different components
    /// </summary>
    public IEnumerable<TNode>[] Components { get; }
    /// <summary>
    /// Set finder that stores relationships between nodes. If two nodes in same set they in same component.
    /// </summary>
    public UnionFind SetFinder { get; }
    
    ///<inheritdoc/>
    public void Dispose()
    {
        SetFinder.Dispose();
    }
    /// <summary>
    /// Function that helps to determine whatever two nodes in same component
    /// </summary>
    public bool InSameComponent(int nodeId1,int nodeId2){
        return SetFinder.FindSet(nodeId1)==SetFinder.FindSet(nodeId2);
    }
}