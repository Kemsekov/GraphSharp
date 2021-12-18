using GraphSharp.Children;
using GraphSharp.Nodes;

public class NodeConnector : Child
{
    public NodeConnector(INode node, INode parent) : base(node)
    {
        Parent = parent;
        if(node is NodeXY n1 && parent is NodeXY n2)
            Weight = Math.Sqrt((n1.X-n2.X)*(n1.X-n2.X)+(n1.Y-n2.Y)*(n1.Y-n2.Y));
    }
    public INode Parent{get;init;}
    public double Weight{get;init;} = 1;
}