using GraphSharp.Nodes;

namespace GraphSharp.Children
{
    public class Child : IChild
    {
        public Child(INode node)
        {
            this.Node = node;
        }
        public INode Node{get;init;}
        public int CompareTo(IChild other) => Node.CompareTo(other.Node);
        public override string ToString()
        {
            return $"Child to node {Node.Id}";
        }
    }
}