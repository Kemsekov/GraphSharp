
using System.Collections.Concurrent;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

/// <summary>
/// works only with
/// </summary>
public class AllPathFinder : IVisitor<NodeXY,NodeConnector>
{
    byte[] _visited;
    public IList<INode> Path;
    public bool PathDone = false;
    /// <summary>
    /// _trace[node] = parent
    /// </summary>
    IDictionary<INode,INode> _trace = new ConcurrentDictionary<INode,INode>();
    public AllPathFinder(int nodesCount)
    {
        _visited = new byte[nodesCount];
        Path = new List<INode>(nodesCount);
    }
    public void EndVisit()
    {

    }

    public bool Select(NodeConnector edge)
    {
        var n = edge.Node;
        return Path.Count==0 || n.Id==Path.Last().Id;
    }
    public void Visit(NodeXY node)
    {
        if(PathDone) return;

        _visited[node.Id] = 1;
        foreach(var n in node.Edges){
            if(_visited[n.Node.Id]==0){
                _trace[n.Node] = node;
                Path.Add(n.Node);
                return;
            }
        }
        if(_trace.TryGetValue(node,out var parent))
            Path.Add(parent);
        else
            PathDone = true;

    }
}