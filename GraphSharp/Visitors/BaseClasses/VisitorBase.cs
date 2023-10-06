using System;
namespace GraphSharp.Visitors;

/// <summary>
/// Base class for visitors that contains default extension capabilities and tracking
/// of algorithm execution steps count and algorithm completion.
/// </summary>
public abstract class VisitorBase<TEdge> : IVisitorWithSteps<TEdge>
where TEdge : IEdge
{
    /// <summary>
    /// This predicate can be used to avoid some edges/nodes in path finding. 
    /// By default pass all edges. <br/>
    /// Begin called at the beginning of <see cref="Select"/> method.<br/>
    /// If <see langword="Condition(edge)"/> is <see langword="true"/> this 
    /// <see langword="edge"/> is allowed to pass <see langword="Select"/> function,
    /// else not allowed.
    /// </summary>
    public Predicate<EdgeSelect<TEdge>> Condition { get; set;}
    /// <summary>
    /// Called at the beginning of <see cref="Visit"/> method
    /// </summary>
    public event Action<int> VisitEvent;
    /// <summary>
    /// Called at the beginning of <see cref="Select"/> method
    /// </summary>
    public event Action<EdgeSelect<TEdge>> SelectEvent;
    /// <summary>
    /// Called at the beginning of <see cref="Start"/> method
    /// </summary>
    public event Action StartEvent;
    /// <summary>
    /// Called at the beginning of <see cref="End"/> method
    /// </summary>
    public event Action EndEvent;
    ///<inheritdoc/>
    public bool Done{get;set;} = false;
    ///<inheritdoc/>
    public int Steps{get;set;} = 0;
    ///<inheritdoc/>
    public bool DidSomething{get;set;}
    ///<inheritdoc/>
    public VisitorBase()
    {
        Condition = edge=>true;
        VisitEvent = node=>{};
        SelectEvent = edge=>{};
        StartEvent = ()=>{};
        EndEvent = ()=>{};
    }
    ///<inheritdoc/>
    public void Start(){
        DidSomething = false;
        StartEvent();
        StartImpl();
    }
    ///<inheritdoc/>
    public bool Select(EdgeSelect<TEdge> edge){
        if(!Condition(edge) || Done) return false;
        SelectEvent(edge);
        return SelectImpl(edge);
    }
    ///<inheritdoc/>
    public void Visit(int node){
        if(Done) return;
        VisitEvent(node);
        VisitImpl(node);
    }
    ///<inheritdoc/>
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
    protected abstract bool SelectImpl(EdgeSelect<TEdge> edge);
    /// <summary>
    /// <see cref="Visit"/> function implementation
    /// </summary>
    protected abstract void VisitImpl(int node);
    /// <summary>
    /// <see cref="End"/> function implementation
    /// </summary>
    protected abstract void EndImpl();
}