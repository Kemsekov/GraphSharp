namespace GraphSharp.Visitors;
public interface IVisitorWithSteps<TNode, TEdge> : IVisitor<TNode,TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Indicates if algorithm is done and do not need any further executions
    /// </summary>
    bool Done {get;}
    /// <summary>
    /// Indicates if algorithm done anything in last it's step.
    /// </summary>
    bool DidSomething{get;}
    /// <summary>
    /// Count of how many steps executed so far
    /// </summary>
    /// <value></value>
    int Steps{get;}
}