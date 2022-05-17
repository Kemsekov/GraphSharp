using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public interface INodeSource<TNode> : IEnumerable<TNode>
    where TNode : INode
    {
        int Count{ get; }
        /// <summary>
        /// Returns max id value of all nodes.
        /// </summary>
        /// <value></value>
        int MaxNodeId {get;}
        /// <summary>
        /// Returns min id value of all nodes.
        /// </summary>
        /// <value></value>
        int MinNodeId {get;}
        void Add(TNode node);
        bool Remove(TNode node);
        bool Remove(int nodeId);
        TNode this[int nodeId] {get;set;}
        bool TryGetNode(int nodeId, out TNode node);
        void Clear();
    }
}