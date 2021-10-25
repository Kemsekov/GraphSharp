using System;
using GraphSharp.Nodes;

namespace GraphSharp
{
    public readonly struct NodeValue<TValue> : IComparable<NodeValue<TValue>>, IChild
    {
        public readonly TValue Value;
        public readonly NodeBase<TValue> NodeBase;
        INode IChild.NodeBase => NodeBase;

        public NodeValue(NodeBase<TValue> nodeBase,TValue value)
        {
            Value = value;
            NodeBase = nodeBase;
        }


        public int CompareTo(NodeValue<TValue> nodeValue)=>NodeBase.CompareTo(nodeValue.NodeBase);
        public int CompareTo(IChild other)=>NodeBase.CompareTo(other.NodeBase);
    }
}