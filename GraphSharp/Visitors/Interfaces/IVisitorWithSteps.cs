namespace GraphSharp.Visitors;
/// <summary>
/// Visitor that counts it's steps and completion state
/// </summary>
public interface IVisitorWithSteps<TNode, TEdge> : IVisitor<TNode,TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Indicates if algorithm is done and do not need any further executions
    /// </summary>
    bool Done {get;}
    /// <summary>
    /// Count of how many steps executed so far
    /// </summary>
    /// <value></value>
    int Steps{get;}
    /// <summary>
    /// Whatever given algorithm did anything in last iteration
    /// </summary>
    public bool DidSomething{get;set;}
}