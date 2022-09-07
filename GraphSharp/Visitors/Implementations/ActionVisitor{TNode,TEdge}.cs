using System;
namespace GraphSharp.Visitors;

/// <summary>
/// <see cref="IVisitor{,}"/> implementation that uses lambda functions.
/// </summary>
public class ActionVisitor<TNode, TEdge> : VisitorBase<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{

    /// <param name="visit"><see cref="IVisitor{,}.Visit"/> function</param>
    /// <param name="select"><see cref="IVisitor{,}.Select"/> function</param>
    /// <param name="end"><see cref="IVisitor{,}.End"/> function.</param>
    /// <param name="start"><see cref="IVisitor{,}.Start"/> function.</param>
    public ActionVisitor(Action<TNode>? visit = null, Predicate<TEdge>? select = null, Action? end = null, Action? start = null)
    {
        this.VisitEvent += visit ?? new Action<TNode>(node => { });
        this.Condition = select ?? new Predicate<TEdge>(edge => true);
        this.EndEvent += end ?? new Action(() => { });
        this.StartEvent += start ?? new Action(() => { });
    }
}