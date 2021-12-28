using GraphSharp.Nodes;

namespace GraphSharp.Children
{
    public class Child<T> : Child
    {
        public Child(INode node, T value) : base(node)
        {
            Value = value;
        }
        public T Value { get; protected set; }
    }
}