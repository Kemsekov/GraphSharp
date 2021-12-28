using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

/// <summary>
/// this visitor will try to visit all nodes from some starting node, and it possible return to starting node
/// </summary>
public class AllNodesPathFinder : IVisitor
{
    public AllNodesPathFinder(NodeXY startingNode)
    {
        StartingNode = startingNode;
    }

    public NodeXY StartingNode { get; }

    public void EndVisit()
    {

    }

    public bool Select(IChild child)
    {
        if(child is NodeConnector con){
        }
        return true;
    }

    public void Visit(INode node)
    {
    }
}