using GraphSharp.Edges;
using GraphSharp.Nodes;

public class NodeConnector : Edge
{
    public NodeConnector(INode node, INode parent) : base(node)
    {
        Parent = parent;
        if(node is NodeXY n1 && parent is NodeXY n2)
            Weight = MathF.Sqrt((float)((n1.X-n2.X)*(n1.X-n2.X)+(n1.Y-n2.Y)*(n1.Y-n2.Y)));
    }
    public override int CompareTo(IEdge other)
    {
        if(other is NodeConnector n)
            return Node.CompareTo(n.Node) + Parent.CompareTo(n.Parent);
        return -1;
    }
    public INode Parent{get;init;}
    public float Weight{get;set;} = 1;
}