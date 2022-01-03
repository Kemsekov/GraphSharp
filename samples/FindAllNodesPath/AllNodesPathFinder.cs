#pragma warning disable
using System.Collections.Concurrent;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

/// <summary>
/// this visitor will try to visit all nodes from some starting node, and if possible return to the starting node
/// </summary>
public class AllNodesPathFinder : IVisitor
{
    /// <summary>
    /// _path[node] = parent 
    /// </summary>
    IList<INode> _path = new List<INode>();
    NodeXY buffer = null;
    public AllNodesPathFinder(NodeXY startingNode, int nodesCount)
    {
        _path.Add(startingNode);
        states = new NodeState[nodesCount];
    }
    public NodeState[] states;
    public void EndVisit()
    {
        if(buffer is null && _path.Count>0){
            states[_path.Last().Id] = NodeState.Visited;
            _path.Remove(_path.Last());
            return;
        }
        if(buffer is not null)
            _path.Add(buffer);
        buffer = null;
    }

    //add node-parent to path. If it is already exists, then return
    public bool Select(IEdge edge)
    {
        var currentState = states[edge.Node.Id];
        states[edge.Node.Id] = NodeState.Visited;
        if(currentState == NodeState.AddedToPath) return false;

        var parent = edge.Node as NodeXY;
        var needVisit = parent.Id==_path.Last().Id;
        
        buffer ??= edge.Node as NodeXY;
        var bufferState = states[buffer.Id];

        if(edge is NodeConnector c){
            lock(buffer)
            if(c.Node is NodeXY current){
                if(parent.Distance(current)<parent.Distance(buffer)){
                    if(currentState==bufferState || currentState == NodeState.None)
                        buffer = current;
                    return needVisit;
                }
            }
        }
        return needVisit;
    }

    /*
    Visit one node per Step.
    Right in the beggining test this node state to visited.
    Select any node from it's edgeren that are not added to path
    Select any node that not visited, if there is no one, then
    Select any visited
    If there is no one, then roll back to previous node and return.
    If found such a node, add it to path and set it's state to AddedToPath.
    Set this node to be _next_node and set _previous_node to be current one.

    */
    public void Visit(INode node)
    {
        states[node.Id] = NodeState.AddedToPath;
        _path.Add(node);
    }
    public IList<INode> GetPath()
    {
        return _path;
    }
}