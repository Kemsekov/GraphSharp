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
        //lineGraph.Nodes <- N
        var nodes = new Dictionary<int,InverseLineGraphNode>(lineGraph.Nodes.Count());
        foreach(var n in lineGraph.Nodes)
            nodes[n.Id] = new(-1,-1);
        
        var gNodes = new DefaultNodeSource<TNode>();
        var gEdges = new DefaultEdgeSource<InverseLineGraphEdge>();

        var randomEdge = lineGraph.Edges.First();
        var n1 = randomEdge.SourceId;
        var n2 = randomEdge.TargetId;
        


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