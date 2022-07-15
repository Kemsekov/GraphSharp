using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.Graphs
{
    public interface INodeSource<TNode> : IEnumerable<TNode>
    where TNode : INode
    {
        int Count{ get; }
        /// <summary>
        /// Returns max id value of all nodes. If there is no nodes, returns -1.
        /// </summary>
        /// <value></value>
        int MaxNodeId {get;}
        /// <summary>
        /// Returns min id value of all nodes. If there is no nodes, returns -1.
        /// </summary>
        /// <value></value>
        int MinNodeId {get;}
        void Add(TNode node);
        bool Remove(TNode node);
        bool Remove(int nodeId);
        TNode this[int nodeId] {get;set;}
        bool TryGetNode(int nodeId, out TNode? node);
        void Clear();
    }
}