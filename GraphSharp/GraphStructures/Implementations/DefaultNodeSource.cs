using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GraphSharp.Exceptions;

namespace GraphSharp.Graphs;

/// <summary>
/// Default implementation of <see cref="INodeSource{TNode}"/>
/// </summary>
public class DefaultNodeSource<TNode> : INodeSource<TNode>
where TNode : INode
{
    IDictionary<int, TNode> Nodes;
    ///<inheritdoc/>
    public TNode this[int nodeId]
    {
        get => Nodes[nodeId]; 
        set
        {
            if(value.Id!=nodeId) throw new WrongNodeSetException($"{value} have Id {value.Id} which is not equal to {nodeId}. Cannot set it.");
            Nodes[nodeId] = value;
        }
    }
    ///<inheritdoc/>
    public int Count => Nodes.Count;
    ///<inheritdoc/>
    public int MaxNodeId { get; protected set; }
    ///<inheritdoc/>
    public int MinNodeId { get; protected set; }

    ///<inheritdoc/>
    public bool IsReadOnly => false;
    /// <summary>
    /// Creates a new instance of node source and fills it with given nodes
    /// </summary>
    public DefaultNodeSource(IEnumerable<TNode> nodes) : this()
    {
        foreach(var n in nodes){
            Add(n);
        }
    }

    ///<inheritdoc/>
    public DefaultNodeSource()
    {
        MaxNodeId = -1;
        MinNodeId = -1;
        Nodes = new ConcurrentDictionary<int, TNode>(Environment.ProcessorCount, 0);
    }
    void UpdateMaxMinNodeId(int nodeId)
    {
        if (nodeId > MaxNodeId)
            MaxNodeId = nodeId;
        if (nodeId < MinNodeId || MinNodeId == -1)
            MinNodeId = nodeId;
    }
    ///<inheritdoc/>
    public void Add(TNode node)
    {
        Nodes[node.Id] = node;
        UpdateMaxMinNodeId(node.Id);
    }

    ///<inheritdoc/>
    public IEnumerator<TNode> GetEnumerator()
    {
        return Nodes.Values.GetEnumerator();
    }

    ///<inheritdoc/>
    public bool Remove(TNode node)
    {
        return Remove(node.Id);
    }

    ///<inheritdoc/>
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

    ///<inheritdoc/>
    public bool TryGetNode(int nodeId, out TNode? node)
    {
        return Nodes.TryGetValue(nodeId, out node);
    }
    ///<inheritdoc/>
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
            MinNodeId = Nodes.MinBy(x=>x.Key).Key;
        if (removedNodeId == MaxNodeId)
            MaxNodeId = Nodes.MaxBy(x=>x.Key).Key;
    }

    ///<inheritdoc/>
    public bool Move(TNode node, int newId)
    {
        if (TryGetNode(newId, out var _)) return false;
        if (!Remove(node)) return false;
        node.Id = newId;
        Add(node);
        return true;
    }

    ///<inheritdoc/>
    public bool Move(int nodeId, int newId)
    {
        if (TryGetNode(nodeId, out var n) && n is not null)
            return Move(n, newId);
        return false;
    }

    ///<inheritdoc/>
    public bool Contains(int nodeId)
    {
        return TryGetNode(nodeId, out var _);
    }

    ///<inheritdoc/>
    public bool Contains(TNode node)
    {
        return Contains(node.Id);
    }

    ///<inheritdoc/>
    public void CopyTo(TNode[] array, int arrayIndex)
    {
        foreach (var n in Nodes)
        {
            array[arrayIndex] = n.Value;
            arrayIndex++;
        }
    }
}