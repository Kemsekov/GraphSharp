using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
namespace GraphSharp.Graphs;

class InverseLineGraphNode
{
    public InverseLineGraphNode(int sourceId, int targetId)
    {
        SourceId = sourceId;
        TargetId = targetId;
    }

    public int SourceId { get; set; }
    public int TargetId { get; set; }
    public InverseLineGraphNode Clone()
    {
        return new(SourceId, TargetId);
    }
}
/// <summary>
/// Edge that contains original node that was used to build an inverse line graph and 
/// source and target id it was assigned
/// </summary>
public class InverseLineGraphEdge : IEdge
{
    /// <summary>
    /// Creates new instance of inverse line graph edge
    /// </summary>
    public InverseLineGraphEdge(INode baseNode, int sourceId, int targetId)
    {
        this.BaseNode = baseNode;
        SourceId = sourceId;
        TargetId = targetId;
    }
    /// <inheritdoc/>
    public int SourceId { get; set; }
    /// <inheritdoc/>
    public int TargetId { get; set; }
    /// <inheritdoc/>
    public double Weight { get => BaseNode.Weight; set => BaseNode.Weight = value; }
    /// <inheritdoc/>
    public Color Color { get => BaseNode.Color; set => BaseNode.Color = value; }
    /// <summary>
    /// Base node that was used to build this edge
    /// </summary>
    public INode BaseNode { get; }
    /// <inheritdoc/>
    public IEdge Clone()
    {
        return new InverseLineGraphEdge(BaseNode, SourceId, TargetId);
    }
}
/// <summary>
/// Default inverse line graph configuration
/// </summary>
public class InverseLineGraphConfiguration<TNode, TEdge> : IGraphConfiguration<TNode, InverseLineGraphEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Creates new instance of inverse line graph configuration
    /// </summary>
    public InverseLineGraphConfiguration(IGraphConfiguration<TNode, TEdge> graph)
    {
        Rand = graph.Rand;
        this._graph = graph;
    }
    /// <inheritdoc/>
    public Random Rand { get; set; }

    private IGraphConfiguration<TNode, TEdge> _graph;
    /// <inheritdoc/>

    public InverseLineGraphEdge CreateEdge(TNode source, TNode target)
    {
        return new InverseLineGraphEdge(source, source.Id, target.Id);
    }
    /// <inheritdoc/>

    public IEdgeSource<InverseLineGraphEdge> CreateEdgeSource()
    {
        return new DefaultEdgeSource<InverseLineGraphEdge>();
    }
    /// <inheritdoc/>

    public TNode CreateNode(int nodeId)
    {
        return _graph.CreateNode(nodeId);
    }
    /// <inheritdoc/>

    public INodeSource<TNode> CreateNodeSource()
    {
        return _graph.CreateNodeSource();
    }
}

/// <summary>
/// Algorithm that tries to build inverse line graph
/// </summary>
public class InverseLineGraph<TNode, TEdge> : IImmutableGraph<TNode, InverseLineGraphEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Creates new instance of inverse line graph
    /// </summary>
    public InverseLineGraph(IImmutableGraph<TNode, TEdge> lineGraph, IGraphConfiguration<TNode, InverseLineGraphEdge>? configuration = null)
    {
        Configuration = configuration ?? new InverseLineGraphConfiguration<TNode, TEdge>(lineGraph.Configuration);
        var lineEdges = lineGraph.Edges;
        //lineGraph.Nodes <- N
        var nodes = new Dictionary<int,InverseLineGraphNode>(lineGraph.Nodes.Count());
        foreach(var n in lineGraph.Nodes)
            nodes[n.Id] = new(-1,-1);
        
        var gNodes = new DefaultNodeSource<TNode>();
        var gEdges = new DefaultEdgeSource<InverseLineGraphEdge>();

        var randomEdge = lineGraph.Edges.First();
        var n1 = randomEdge.SourceId;
        var n2 = randomEdge.TargetId;
        var v1 = 0;
        var v2 = 1;
        gNodes.Add(Configuration.CreateNode(v1));
        gNodes.Add(Configuration.CreateNode(v2));
        nodes[n1] = new InverseLineGraphNode(v1,v2);
        nodes[n2] = new InverseLineGraphNode(v1,-1);
        var toConnect = lineEdges.Neighbors(n1).Except(lineEdges.Neighbors(n2).Concat(new[]{n2}));
        foreach(var n in toConnect)
        {
            AddAdjacentNode(nodes, n, v2);
        }
        var n1Neighbors = lineEdges.Neighbors(n1);
        var n2Neighbors = lineEdges.Neighbors(n2);
        var intersection = n1Neighbors.Intersect(n2Neighbors).ToList();
        if(intersection.Count<3){
            var nu = intersection.FirstOrDefault(nu=>{
                var nuNeighbors = lineEdges.Neighbors(nu);
                var firstCondition = nuNeighbors.Except(n1Neighbors).Except(n2Neighbors).Count()==0;
                var secondCondition = nuNeighbors.Except(new int[]{n1,n2}).Count()>=3;
                return firstCondition && secondCondition;
            },-1);
            if(nu!=-1){
                AddAdjacentNode(nodes,nu,v2);
                intersection = intersection.Except(new int[]{nu}).ToList();
            }   
            else{
                InitSpecialCases();
            }
        }
        else{
            
        }
    }

    void AddAdjacentNode(Dictionary<int, InverseLineGraphNode> nodes, int nodeInLineGraph, int adjacentNode)
    {
        var connection = nodes[nodeInLineGraph];
        if (connection.SourceId == -1)
            connection.SourceId = adjacentNode;
        else
            connection.TargetId = adjacentNode;
        (var n1, var n2) = (Math.Min(connection.SourceId,connection.TargetId),Math.Max(connection.SourceId,connection.TargetId));
        connection.SourceId = n1;
        connection.TargetId = n2;
    }

    private void InitSpecialCases()
    {
        throw new NotImplementedException();
    }

    bool AllHaveAtLeastOneFreeSpace(IEnumerable<int> clique, Dictionary<int, InverseLineGraphNode> inverseNodes){
        return clique.All(x =>
        {
            var tmp = inverseNodes[x];
            return tmp.SourceId == -1 || tmp.TargetId == -1;
        });
    }
    bool HaveAtMostOneOccupiedSpace(IEnumerable<int> clique, Dictionary<int, InverseLineGraphNode> inverseNodes){
        int count = 0;
        return clique.All(x =>
        {
            var tmp = inverseNodes[x];
            if(tmp.SourceId == -1 && tmp.TargetId == -1) return true;
            count++;
            return count==1;
        });
    }
    void Fill(IEnumerable<int> clique, Dictionary<int, InverseLineGraphNode> inverseNodes, ref int index)
    {
        foreach (var c in clique)
        {
            var tmp = inverseNodes[c];
            if (tmp.SourceId == -1)
                tmp.SourceId = index;
            else
                tmp.TargetId = index;
        }
        index++;
    }

    /// <inheritdoc/>

    public IImmutableNodeSource<TNode> Nodes { get; }
    /// <inheritdoc/>

    public IImmutableEdgeSource<InverseLineGraphEdge> Edges { get; }
    /// <inheritdoc/>

    public IGraphConfiguration<TNode, InverseLineGraphEdge> Configuration { get; }
    /// <inheritdoc/>

    public ImmutableGraphOperation<TNode, InverseLineGraphEdge> Do => new(this);
    /// <inheritdoc/>

    public ImmutableGraphConverters<TNode, InverseLineGraphEdge> Converter => new(this);
}