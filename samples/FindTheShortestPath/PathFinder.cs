using System.Collections.Concurrent;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
public class PathFinder : IVisitor
{
    /// <summary>
    /// _path[node] = parent 
    /// </summary>
    IDictionary<INode, INode> _path = new ConcurrentDictionary<INode, INode>();
    /// <summary>
    /// what is the length of path from startNode to some other node so far.  
    /// </summary>
    IDictionary<INode, double> _pathLength = new ConcurrentDictionary<INode, double>();
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

    public bool Select(IEdge Edge)
    {
        bool updatePath = true;
        if (Edge is NodeConnector connection)
        {
            var pathLength = _pathLength[connection.Parent] + connection.Weight;

            if (_pathLength.TryGetValue(connection.Node, out double pathSoFar))
            {
                if (pathSoFar <= pathLength)
                {
                    updatePath = false;
                }
            }
            if (updatePath)
            {
                _pathLength[connection.Node] = pathLength;
                _path[connection.Node] = connection.Parent;
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
    public double GetPathLength(INode node){
        if(_pathLength.TryGetValue(node,out double length)){
            return length;
        }
        return 0;
    }
}