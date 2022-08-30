using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Common;


namespace GraphSharp.Graphs;
/// <summary>
/// Default implementation of <see cref="INodeSource{}"/>
/// </summary>
public class DefaultNodeSource<TNode> : INodeSource<TNode>
where TNode : INode
{
    IDictionary<int, TNode> Nodes;
    public TNode this[int nodeId] { get => Nodes[nodeId]; set => Nodes[nodeId] = value; }
    public int Count => Nodes.Count;
    public int MaxNodeId { get; protected set; }
    public int MinNodeId { get; protected set; }
    public DefaultNodeSource()
    {
        MaxNodeId = -1;
        MinNodeId = -1;
        Nodes = new ConcurrentDictionary<int, TNode>(Environment.ProcessorCount,0);
    }
    public void Add(TNode node)
    {
        Nodes[node.Id] = node;
        if (node.Id > MaxNodeId)
            MaxNodeId = node.Id;
        if (node.Id < MinNodeId || MinNodeId == -1)
            MinNodeId = node.Id;
    }

    public IEnumerator<TNode> GetEnumerator()
    {
        foreach (var n in Nodes)
            yield return n.Value;
    }

    public bool Remove(TNode node)
    {
        return Remove(node.Id);
    }

    public bool Remove(int nodeId)
    {
        if (Nodes.Remove(nodeId))
        {
            UpdateMaxMinNodeIdAfterRemovedNode(nodeId);
            return true;
        }
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public bool TryGetNode(int nodeId, out TNode? node)
    {
        return Nodes.TryGetValue(nodeId, out node);
    }
    public void Clear()
    {
        Nodes.Clear();
        MaxNodeId = -1;
        MinNodeId = -1;
    }
    void UpdateMaxMinNodeIdAfterRemovedNode(int removedNodeId)
    {
        if (Nodes.Count == 0)
        {
            MinNodeId = -1;
            MaxNodeId = -1;
            return;
        }

        //Dictionary is sorted by key when key is integer by default
        //Update max node id or min node id only if removed node is one of them
        if (removedNodeId == MinNodeId)
            MinNodeId = Nodes.First().Key;
        if (removedNodeId == MaxNodeId)
            MaxNodeId = Nodes.Last().Key;
    }

    public bool Move(TNode node, int newId)
    {
        if(TryGetNode(newId,out var _)) return false;
        if(!Remove(node)) return false;
        node.Id = newId;
        Add(node);
        return true;
    }

    public bool Move(int nodeId, int newId)
    {
        if(TryGetNode(nodeId,out var n) && n is not null)
            return Move(n,newId);
        return false;
    }

    public bool Contains(int nodeId)
    {
        return TryGetNode(nodeId,out var _);
    }

    public bool Contains(TNode node)
    {
        return Contains(node.Id);
    }
}