using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;

namespace GraphSharp.Visitors;
/// <summary>
/// Base class for path finding algorithms, that search for all paths from <see cref="StartNodeId"/> to all other nodes. <br/> 
/// Between each consequent calls of <see langword="Start"/> and 
/// <see langword="End"/> parameter <paramref langword="DidSomething"/> 
/// must be set to <paremref name="true"/> in order for path finder to continue it's working.
/// Logic is that if in the iteration execution of algorithm we did nothing at all
/// that means we are done and algorithm must stop. In that case <paramref langword="Done"/>
/// will be set to true.
/// </summary>
public abstract class PathFinderBase<TNode, TEdge> : VisitorBase<TNode, TEdge>, IDisposable, IPathFinder<TNode>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Path storage array that used to track paths in a graph. It may be:<br/>
    /// <see langword="Path[source] == target"/> <br/>
    /// or <br/>
    /// <see langword="Path[target] == source"/> <br/>
    /// Depending on implementation <br/>
    ///  <see langword="Path[nodeId] == -1"/><br/> 
    /// if algorithm did not found any ancestor for this node.
    /// </summary>
    public RentedArray<int> Path{get;init;}
    public PathType PathType { get; }
    public Func<TEdge,double> GetWeight{get;set;}
    /// <summary>
    /// Id of first node in the path
    /// </summary>
    public int StartNodeId {get;set;}
    /// <summary>
    /// Algorithm executed on this graph
    /// </summary>
    public IImmutableGraph<TNode, TEdge> Graph { get; }

    /// <param name="graph">Graph that will be used to find path on</param>
    public PathFinderBase(IImmutableGraph<TNode,TEdge> graph,PathType pathType)
    {
        this.PathType = pathType;
        this.GetWeight = e=>e.Weight;
        this.Condition = edge=>true;
        this.Graph = graph;
        Path = ArrayPoolStorage.RentArray<int>(graph.Nodes.MaxNodeId+1);
        Path.Fill(-1);
    }
    /// <summary>
    /// Disposes object when collected by GC
    /// </summary>
    ~PathFinderBase(){
        Path.Dispose();
    }
    /// <summary>
    /// Sets all values of <see cref="Path"/> to -1, 
    /// resets <see langword="Done"/> to <see langword="false"/> and
    /// <see langword="Steps"/> to <see langword="0"/>.
    /// </summary>
    public void ClearPaths(){
        Path.Fill(-1);
        Done = false;
        Steps = 0;
    }
    ///<inheritdoc/>
    protected override void StartImpl(){
        DidSomething = false;
    }
    ///<inheritdoc/>
    protected override void EndImpl(){
        if(!DidSomething) Done = true;
    }
    /// <summary>
    /// Tries to get a path between two nodes.
    /// </summary>
    /// <returns>List of nodes if path between two nodes is found, else empty list is returned</returns>
    public IPath<TNode> GetPath(int startNodeId, int endNodeId){
        var path = new List<TNode>();
        if (Path[endNodeId] == -1) return new PathResult<TNode>(x=>Graph.ComputePathCost(x,GetWeight),path,PathType);
        while (true)
        {
            var parent = Path[endNodeId];
            path.Add(Graph.Nodes[endNodeId]);
            if (parent == startNodeId) break;
            endNodeId = parent;
        }
        path.Add(Graph.Nodes[startNodeId]);
        path.Reverse();
        return new PathResult<TNode>(x=>Graph.ComputePathCost(x,GetWeight),path,PathType);
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        Path.Dispose();
    }
}