using System;
namespace GraphSharp.Visitors;

/// <summary>
/// <see cref="IVisitor{,}"/> implementation that uses lambda functions.
/// </summary>
public class ActionVisitor<TNode, TEdge> : IVisitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    private Action<TNode> visit;
    private Predicate<TEdge> select;
    private Action endVisit;
    private Action beforeSelect;

    /// <param name="visit"><see cref="IVisitor{,}.Visit"/> function</param>
    /// <param name="select"><see cref="IVisitor{,}.Select"/> function</param>
    /// <param name="endVisit"><see cref="IVisitor{,}.EndVisit"/> function. You can let it be null.</param>
    /// <param name="beforeSelect"><see cref="IVisitor{,}.BeforeSelect"/> function. You can let it be null.</param>
    public ActionVisitor(Action<TNode> visit, Predicate<TEdge> select,  Action? endVisit = null, Action? beforeSelect = null)
    {
        this.visit = visit;
        this.select = select;
        this.endVisit = endVisit ?? new Action(() => { });
        this.beforeSelect = beforeSelect ?? new Action(() => { });
    }
    public void BeforeSelect() => beforeSelect();
    public bool Select(TEdge edge) => select(edge);
    public void Visit(TNode node) => visit(node);
    public void EndVisit() => endVisit();

}