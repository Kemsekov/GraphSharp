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

        public int MaxNodeId{get;protected set;}

        public int MinNodeId{get;protected set;}

        public DefaultNodeSource(int capacity)
        {
            Nodes = new ConcurrentDictionary<int,TNode>(Environment.ProcessorCount,capacity);
        }
        public void Add(TNode node)
        {
            Nodes.Add(node.Id,node);
            UpdateMaxMinNodeId(node.Id);
        }

        public IEnumerator<TNode> GetEnumerator()
        {
            foreach(var n in Nodes)
                yield return n.Value;
        }

        public bool Remove(TNode node)
        {
            bool removed = Nodes.Remove(node.Id);
            if(removed) UpdateMaxMinNodeId(node.Id);
            return removed;
        }

        public bool Remove(int nodeId)
        {
            bool removed = Nodes.Remove(nodeId);
            if(removed) UpdateMaxMinNodeId(nodeId);
            return removed;
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

        void UpdateMaxMinNodeId(int id){
            MaxNodeId = Math.Max(MaxNodeId,id);
            MinNodeId = Math.Min(MinNodeId,id);
        }
    }
}