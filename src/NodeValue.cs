using System;
using GraphSharp.Nodes;

namespace GraphSharp
{
    public struct NodeValue<T> : IComparable<NodeValue<T>>
    {
        public T Value;
        public NodeBase<T> NodeBase;

        public NodeValue(NodeBase<T> nodeBase,T value)
        {
            Value = value;
            NodeBase = nodeBase;
        }

        public int CompareTo(NodeValue<T> nodeValue)
        {
            return NodeBase.CompareTo(nodeValue.NodeBase);
        }
    }
}