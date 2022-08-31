namespace GraphSharp.Adapters;

/// <summary>
/// Adapter for edges from GraphSharp to work as edges from QuikGraph
/// </summary>
public class EdgeAdapter<TVertex, TEdge> : QuikGraph.IEdge<TVertex>
where TVertex : INode
where TEdge : IEdge
{
    public TVertex Source { get; }
    public TVertex Target { get; }
    public TEdge GraphSharpEdge { get; }
    public GraphSharp.Graphs.IGraph<TVertex, TEdge> Graph { get; }
    public EdgeAdapter(TEdge edge, GraphSharp.Graphs.IGraph<TVertex, TEdge> graph)
    {
        GraphSharpEdge = edge;
        Graph = graph;
        Source = graph.Nodes[edge.SourceId];
        Target = graph.Nodes[GraphSharpEdge.TargetId];
    }
    public override bool Equals(object? obj)
    {
        if (obj is EdgeAdapter<TVertex, TEdge> e)
        {
            return e.GraphSharpEdge.Equals(GraphSharpEdge);
        }
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return GraphSharpEdge.GetHashCode();
    }
}