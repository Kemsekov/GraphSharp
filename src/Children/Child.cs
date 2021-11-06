using GraphSharp.Nodes;

namespace GraphSharp.Children
{
    public class Child : IChild
    {
        public Child(INode node)
        {
            this.Node = node;
        }
        public INode Node{get;}
        public int CompareTo(IChild other) => Node.CompareTo(other.Node);
    }
}