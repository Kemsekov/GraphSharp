namespace GraphSharp.Graphs;


public partial class ImmutableGraphOperation<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Apply linear-time greedy graph nodes coloring algorithm.<br/>
    /// </summary>
    public ColoringResult GreedyColorNodes()
    {
        return new GreedyColoring<TNode, TEdge>(Nodes, Edges).Color();
    }
    /// <summary>
    /// Slightly different implementation of DSatur coloring algorithm.<br/>
    /// A lot better than greedy algorithm and just about a half of it's speed.
    /// </summary>
    public ColoringResult DSaturColorNodes()
    {
        return new DSaturColoring<TNode, TEdge>(Nodes, Edges).Color();
    }
    /// <summary>
    /// Recursive largest first algorithm. The most efficient in colors used algorithm,
    /// but the slowest one.
    /// </summary>
    public ColoringResult RLFColorNodes()
    {
        return new RLFColoring<TNode, TEdge>(Nodes, Edges).Color();
    }
    /// <summary>
    /// QuikGraph's coloring algorithm
    /// </summary>
    public ColoringResult QuikGraphColorNodes()
    {
        return new QuickGraphColoring<TNode, TEdge>(Nodes, Edges).Color();
    }
}