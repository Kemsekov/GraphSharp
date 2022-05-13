using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public class DefaultNodeSource<TNode> : INodeSource<TNode>
    where TNode : INode
    {
        IDictionary<int,TNode> Nodes;
        public TNode this[int nodeId] { get => Nodes[nodeId]; set => Nodes[nodeId] = value; }
        public int Count => Nodes.Count;
        public DefaultNodeSource(int capacity)
        {
            Nodes = new ConcurrentDictionary<int,TNode>(Environment.ProcessorCount,capacity);
        }
        public void Add(TNode node)
        {
            Nodes.Add(node.Id,node);
        }

        public IEnumerator<TNode> GetEnumerator()
        {
            foreach(var n in Nodes)
                yield return n.Value;
        }

        public bool Remove(TNode node)
        {
            return Nodes.Remove(node.Id);
        }

        public bool Remove(int nodeId)
        {
            return Nodes.Remove(nodeId);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetNode(int nodeId, out TNode node)
        {
            return Nodes.TryGetValue(nodeId,out node);
        }
        public void Clear(){
            Nodes.Clear();
        }
    }
}