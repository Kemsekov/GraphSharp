
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

/// <summary>
/// works only with
/// </summary>
public class AllPathFinder : IVisitor
{
    byte[] _visited;
    IList<INode> _path;
    public AllPathFinder(INode startingNode,int nodesCount)
    {
        _visited = new byte[nodesCount];
        _path = new List<INode>(nodesCount){startingNode};
    }
    public void EndVisit()
    {

    }

    public bool Select(IEdge edge)
    {
        return edge.Node.Id==_path.Last().Id;
    }

    public void Visit(INode node)
    {
        _visited[node.Id] = 1;
        foreach(var n in node.Edges){
            if(_visited[n.Node.Id]==0){
                _path.Add(n.Node);
                return;
            }
        }
        _path.RemoveAt(_path.Count-1);
    }
}