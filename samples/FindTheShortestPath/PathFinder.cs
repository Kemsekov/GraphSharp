using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
public class PathFinder : IVisitor
{
    /// <summary>
    /// _path[node] = parent 
    /// </summary>
    Dictionary<INode, INode> _path = new();
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    Dictionary<INode, double> _pathLength = new();
    private INode _startNode;

    /// <param name="startNode">Node from which we need to find a shortest path</param>
    public PathFinder(INode startNode)
    {
        this._startNode = startNode;
        _pathLength[startNode] = 0;
    }
    public void EndVisit()
    {
    }

    public bool Select(IChild child)
    {
        bool updatePath = true;
            if (child is NodeConnector connection)
            {
                var pathLength = _pathLength[connection.Parent] + connection.Weight;

                if (_pathLength.TryGetValue(connection.Node, out double pathSoFar))
                {
                    if (pathSoFar <= pathLength)
                    {
                        updatePath = false;
                    }
                }
                if(updatePath){
                    lock (_path){
                        _pathLength[connection.Node] = pathLength;
                        _path[connection.Node] = connection.Parent;
                    }
                }
            }
        return true;
    }

    public void Visit(INode node)
    {
        //do nothing. We do not actually need to do anything here.
    }

    /// <param name="end"></param>
    /// <returns>Null if path not found</returns>
    public List<INode>? GetPath(INode end)
    {
        if (!_path.ContainsKey(end)) return null;
        var path = new List<INode>();
        while (true)
            if (_path.TryGetValue(end, out INode? parent))
            {
                path.Add(end);
                end = parent;
            }
            else break;
        path.Add(_startNode);
        path.Reverse();
        return path;
    }
    public double GetPathLength(INode node) => _pathLength[node];
}