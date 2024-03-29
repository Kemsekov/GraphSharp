using System;
namespace GraphSharp.Visitors;

/// <summary>
/// <see cref="IVisitor{TEdge}"/> implementation that uses lambda functions.
/// </summary>
public class ActionVisitor<TNode, TEdge> : VisitorBase<TEdge>
where TNode : INode
where TEdge : IEdge
{

    /// <param name="visit"><see cref="IVisitor{TEdge}.Visit"/> function</param>
    /// <param name="select"><see cref="IVisitor{TEdge}.Select"/> function</param>
    /// <param name="end"><see cref="IVisitor{TEdge}.End"/> function.</param>
    /// <param name="start"><see cref="IVisitor{TEdge}.Start"/> function.</param>
    public ActionVisitor(Action<int>? visit = null, Predicate<EdgeSelect<TEdge>>? select = null, Action? end = null, Action? start = null)
    {
        this.VisitEvent += visit ?? new Action<int>(node => { });
        this.Condition = select ?? new Predicate<EdgeSelect<TEdge>>(edge => true);
        this.EndEvent += end ?? new Action(() => { });
        this.StartEvent += start ?? new Action(() => { });
    }
    /// <summary>
    /// End function implementation
    /// </summary>
    protected override void EndImpl()
    {
    }
    /// <summary>
    /// Select function implementation
    /// </summary>
    protected override bool SelectImpl(EdgeSelect<TEdge> edge)
    {
        return true;
    }
    /// <summary>
    /// Start function implementation
    /// </summary>
    protected override void StartImpl()
    {
    }
    /// <summary>
    /// Visit function implementation
    /// </summary>
    protected override void VisitImpl(int node)
    {
    }
}