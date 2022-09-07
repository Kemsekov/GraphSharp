namespace GraphSharp.Visitors;
/// <summary>
/// Contains method to control execution of algorithms on a graph.
/// </summary>
public interface IVisitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// First function to call in each algorithm step.
    /// </summary>
    void Start();
    /// <summary>
    /// Method to process and filter edges that will be passed forward for execution
    /// </summary>
    /// <param name="edge">Edge to process</param>
    /// <returns>True if we need to step into given edge, else false</returns>
    bool Select(TEdge edge);
    /// <summary>
    /// Method to process nodes that was passed by selected edges.
    /// </summary>
    /// <param name="node">Node to process</param>
    void Visit(TNode node);
    /// <summary>
    /// After each select and visit iterations are done this method will do clean-up
    /// or states changes or any specific task at the end of algorithm step
    /// </summary>
    void End();
}