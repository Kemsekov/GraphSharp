using System;
using GraphSharp.Nodes;

namespace GraphSharp
{
    public struct NodeValue<TValue> : IComparable<NodeValue<TValue>>
    {
        public TValue Value;
        public NodeBase<TValue> NodeBase;

        public NodeValue(NodeBase<TValue> nodeBase,TValue value)
        {
            Value = value;
            NodeBase = nodeBase;
        }

        public int CompareTo(NodeValue<TValue> nodeValue)
        {
            return NodeBase.CompareTo(nodeValue.NodeBase);
        }
    }
}