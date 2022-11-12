using System;
namespace GraphSharp.Visitors;

/// <summary>
/// Base class for visitors that contains default extension capabilities and tracking
/// of algorithm execution steps count and algorithm completion.
/// </summary>
public abstract class VisitorBase<TNode, TEdge> : IVisitorWithSteps<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// This predicate can be used to avoid some edges/nodes in path finding. 
    /// By default pass all edges. <br/>
    /// Begin called at the beginning of <see cref="Select"/> method.
    /// </summary>
    public Predicate<TEdge> Condition { get; set;}
    /// <summary>
    /// Called at the beginning of <see cref="Visit"/> method
    /// </summary>
    public event Action<TNode> VisitEvent;
    /// <summary>
    /// Called at the beginning of <see cref="Select"/> method
    /// </summary>
    public event Action<TEdge> SelectEvent;
    /// <summary>
    /// Called at the beginning of <see cref="Start"/> method
    /// </summary>
    public event Action StartEvent;
    /// <summary>
    /// Called at the beginning of <see cref="End"/> method
    /// </summary>
    public event Action EndEvent;
    public bool Done{get;set;} = false;
    public int Steps{get;set;} = 0;
    public bool DidSomething{get;set;}
    public VisitorBase()
    {
        Condition = edge=>true;
        VisitEvent = node=>{};
        SelectEvent = edge=>{};
        StartEvent = ()=>{};
        EndEvent = ()=>{};
    }
    public void Start(){
        DidSomething = false;
        StartEvent();
        StartImpl();
    }
    public bool Select(TEdge edge){
        if(!Condition(edge) || Done) return false;
        SelectEvent(edge);
        return SelectImpl(edge);
    }
    public void Visit(TNode node){
        if(Done) return;
        VisitEvent(node);
        VisitImpl(node);
    }
    public void End(){
        EndEvent();
        EndImpl();
        Steps++;
    }
    /// <summary>
    /// <see cref="Start"/> function implementation
    /// </summary>
    protected abstract void StartImpl();
    /// <summary>
    /// <see cref="Select"/> function implementation
    /// </summary>
    protected abstract bool SelectImpl(TEdge edge);
    /// <summary>
    /// <see cref="Visit"/> function implementation
    /// </summary>
    protected abstract void VisitImpl(TNode node);
    /// <summary>
    /// <see cref="End"/> function implementation
    /// </summary>
    protected abstract void EndImpl();
}