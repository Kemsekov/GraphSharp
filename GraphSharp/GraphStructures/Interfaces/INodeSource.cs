using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphSharp.Common;



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
        /// <summary>
        /// Changes node Id by moving it
        /// </summary>
        /// <returns>true if moved successfully, else false</returns>
        bool Move(TNode node, int newId);
        /// <summary>
        /// Changes node Id by moving it
        /// </summary>
        /// <returns>true if moved successfully, else false</returns>
        bool Move(int nodeId, int newId);
        bool TryGetNode(int nodeId, out TNode? node);
        bool Contains(int nodeId);
        bool Contains(TNode node);
        void Clear();
    }
}