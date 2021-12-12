using GraphSharp.Children;
using GraphSharp.Nodes;

public class NodeConnector : Child
{
    public NodeConnector(INode node, INode parent, double weight) : base(node)
    {
        Parent = parent;
        Weight = weight;
    }
    public INode Parent{get;init;}
    public double Weight{get;init;}

}