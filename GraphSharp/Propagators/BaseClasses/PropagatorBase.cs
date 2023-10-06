using System;
using GraphSharp.Visitors;
using System.Linq;
using GraphSharp.Graphs;
using GraphSharp.Common;

namespace GraphSharp.Propagators;

/// <summary>
/// Advanced graph explorer. <br/> 
/// Uses <see cref="ByteStatesHandler"/> to store node states.<br/> 
/// See <see cref="UsedNodeStates"/> 
/// to access used node states. <br/>
/// By default proceed exploration by out edges. <br/>
/// Uses <see cref="IVisitor{TEdge}"/> to implement exploration logic. <br/>
/// See <see cref="PropagatorBase{TEdge}.Propagate"/> for details about how visitor works with
/// this class.
/// </summary>
public abstract class PropagatorBase<TEdge> : IPropagator<TEdge>, IDisposable
where TEdge : IEdge
{
    /// <summary>
    /// Current working visitor in this propagator. On each call of <see cref="IPropagator{TEdge}.Propagate"/>
    /// this visitor will be used to implement algorithm logic
    /// </summary>
    public IVisitor<TEdge> Visitor { get; protected set; }
    /// <summary>
    /// Graph edges used
    /// </summary>
    public IImmutableEdgeSource<TEdge> Edges { get;protected set; }
    /// <summary>
    /// Max node id
    /// </summary>
    public int MaxNodeId { get; }

    /// <summary>
    /// Biggest used byte node state
    /// </summary>
    public static byte BiggestUsedState => UsedNodeStates.IterateByOutEdges;
    /// <summary>
    /// Underlying node states that used to keep track of exploration process
    /// </summary>
    public ByteStatesHandler NodeStates { get;protected set; }

    /// <param name="visitor">Visitor to use</param>
    /// <param name="edges">Graph edges to use</param>
    /// <param name="maxNodeId">Max node id in graph</param>
    public PropagatorBase(IImmutableEdgeSource<TEdge> edges,IVisitor<TEdge> visitor, int maxNodeId = -1)
    {
        Visitor = visitor;
        Edges = edges;
        MaxNodeId = maxNodeId == -1 ? edges.MaxNodeId() : maxNodeId;
        this.NodeStates = new ByteStatesHandler(MaxNodeId+1);
        NodeStates.SetStateToAll(UsedNodeStates.IterateByOutEdges);
        NodeStates.DefaultState = UsedNodeStates.IterateByOutEdges;
    }
    ///<inheritdoc/>
    public void SetPosition(params int[] nodeIndices)
    {
        NodeStates.RemoveStateFromAll(UsedNodeStates.ToVisit | UsedNodeStates.Visited);
        NodeStates.AddState(UsedNodeStates.Visited, nodeIndices);
    }
    ///<inheritdoc/>
    public void Reset(IImmutableEdgeSource<TEdge> edges, IVisitor<TEdge> visitor, int maxNodeId = -1)
    {
        Visitor = visitor;
        Edges = edges;
        NodeStates.Dispose();
        maxNodeId = maxNodeId==-1 ? edges.MaxNodeId() : maxNodeId;
        NodeStates = new ByteStatesHandler(maxNodeId+1);
        NodeStates.SetStateToAll(UsedNodeStates.IterateByOutEdges);
        NodeStates.DefaultState = UsedNodeStates.IterateByOutEdges;
    }

    /// <summary>
    /// Tells all nodes to iterate by in edges only.<br/>
    /// </summary>
    public void SetToIterateByInEdges()
    {
        NodeStates.AddStateToAll(UsedNodeStates.IterateByInEdges);
        NodeStates.RemoveStateFromAll(UsedNodeStates.IterateByOutEdges);
    }
    /// <summary>
    /// Tells given nodes to iterate by in edges only.<br/>
    /// Do not affect other nodes except of given.
    /// </summary>
    public void SetToIterateByInEdges(params int[] nodes)
    {
        NodeStates.AddState(UsedNodeStates.IterateByInEdges, nodes);
        NodeStates.RemoveState(UsedNodeStates.IterateByOutEdges, nodes);
    }
    /// <summary>
    /// Tells all nodes to iterate by out edges only.<br/>
    /// </summary>
    public void SetToIterateByOutEdges()
    {
        NodeStates.AddStateToAll(UsedNodeStates.IterateByOutEdges);
        NodeStates.RemoveStateFromAll(UsedNodeStates.IterateByInEdges);
    }
    /// <summary>
    /// Tells given nodes to iterate by out edges only.<br/>
    /// Do not affect other nodes except of given.
    /// </summary>
    public void SetToIterateByOutEdges(params int[] nodes)
    {
        NodeStates.AddState(UsedNodeStates.IterateByOutEdges, nodes);
        NodeStates.RemoveState(UsedNodeStates.IterateByInEdges, nodes);
    }
    /// <summary>
    /// Tells all nodes to iterate by out and in edges at the same time.
    /// </summary>
    public void SetToIterateByBothEdges()
    {
        NodeStates.AddStateToAll(UsedNodeStates.IterateByInEdges);
        NodeStates.AddStateToAll(UsedNodeStates.IterateByOutEdges);
    }
    /// <summary>
    /// Tells given nodes to iterate by out and in edges at the same time.
    /// Do not affect other nodes except of given.
    /// </summary>
    public void SetToIterateByBothEdges(params int[] nodes)
    {
        NodeStates.AddState(UsedNodeStates.IterateByInEdges, nodes);
        NodeStates.AddState(UsedNodeStates.IterateByOutEdges, nodes);
    }
    /// <summary>
    /// Propagate nodes. In general it is just taking a step further in graph exploration.<br/>
    /// 1) Calls <see cref="IVisitor{TEdge}.Start"/> to prepare visitor for next iteration<br/>
    /// 2) Finds all nodes with state <see cref="UsedNodeStates.ToVisit"/><br/>
    /// 3) Finds edges that touch these nodes in such a way, that: <br/>
    /// If node have state <see cref="UsedNodeStates.IterateByOutEdges"/>, 
    /// then it chooses all it's out edges <br/>
    /// If node have state <see cref="UsedNodeStates.IterateByInEdges"/>, 
    /// then it chooses all it's in edges <br/>
    /// Meanwhile both in and out edges can be chosen from one node.<br/>
    /// 4) It calls <see cref="IVisitor{TEdge}.Select"/> to all found edges to determine
    /// which direction need to be explored further more, and which are don't need to.<br/>
    /// 5) On each passed edge it founds other node of the edge (direction), and
    /// marks it as <see cref="UsedNodeStates.Visited"/><br/>
    /// 6) Later on it calls <see cref="IVisitor{TEdge}.Visit"/> on each node that marked as 
    /// <see cref="UsedNodeStates.Visited"/> <br/>
    /// 7) Switch all <see cref="UsedNodeStates.Visited"/> nodes states to <see cref="UsedNodeStates.ToVisit"/>
    /// to mark next iteration. <br/>
    /// 8) Calls <see cref="IVisitor{TEdge}.End"/> to signal that execution of iteration is ended. <br/>
    /// Good luck trying to implement any custom visitor!
    /// </summary>
    public void Propagate()
    {
        Visitor.Start();
        PropagateNodes();
        for (int i = 0; i < NodeStates.Length; i++)
        {
            if (NodeStates.IsInState(UsedNodeStates.Visited, i))
            {
                NodeStates.RemoveState(UsedNodeStates.Visited, i);
                NodeStates.AddState(UsedNodeStates.ToVisit, i);
            }
            else
                NodeStates.RemoveState(UsedNodeStates.ToVisit, i);
        }
        Visitor.End();
    }
    /// <summary>
    /// This method implements how to call <see cref="PropagatorBase{TEdge}.PropagateNode"/> on nodes.
    /// </summary>
    protected abstract void PropagateNodes();
    /// <summary>
    /// Iterates trough
    /// </summary>
    protected void PropagateNode(int nodeId, byte state)
    {
        if (ByteStatesHandler.IsInState(UsedNodeStates.IterateByInEdges,state))
            foreach (var edge in Edges.InEdges(nodeId))
            {
                if (!Visitor.Select(new(edge,nodeId))) continue;
                NodeStates.AddState(UsedNodeStates.Visited, edge.SourceId);
            }
        if (ByteStatesHandler.IsInState(UsedNodeStates.IterateByOutEdges,state))
            foreach (var edge in Edges.OutEdges(nodeId))
            {
                if (!Visitor.Select(new(edge,nodeId))) continue;
                NodeStates.AddState(UsedNodeStates.Visited, edge.TargetId);
            }
    }
    ///<inheritdoc/>
    public void Dispose()
    {
        NodeStates.Dispose();
    }


}