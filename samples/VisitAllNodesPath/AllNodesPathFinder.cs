using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

public class AllNodesPathFinder : IVisitor
{
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