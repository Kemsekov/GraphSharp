using System;
using GraphSharp.Visitors;
using System.Linq;
using GraphSharp.Graphs;
using Microsoft.Toolkit.HighPerformance;
using GraphSharp.Common;

namespace GraphSharp.Propagators;

/// <summary>
/// By default this implementation assign a state to each node as <see cref="byte"/> with value of power of 2. 
/// There is 3 states that already used: <paramref name="None"/> = 0, <paramref name="ToVisit"/> = 1, <paramref name="Visited"/> = 2. They available as static members of this class.<br/>
/// So there is only 6 states left for your disposal: 4, 8, 16, 32, 64, 128.<br/>
/// WARNING: if you using to assign or to check states(bytes) which values is not power of 2
/// then you may get unexpected behavior.
/// </summary>
public abstract class PropagatorBase<TNode, TEdge> : IPropagator<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Determines how we iterate trough graph. <br/>
    /// <paramref name="false"/> means we iterating by out edges. <br/>
    /// <paramref name="true"/> means we iterating by in edges (aka in reverse order). <br/>
    /// By default this value is set to <paramref name="false"/> <br/>
    /// Fell free to change in it between calls of <see cref="IPropagator{,}.Propagate"/>. <br/>
    /// Changing it while processing <paramref name="Propagate"/> method will lead to unexpected behavior.
    /// </summary>
    public bool ReverseOrder { get; set; } = false;
    /// <summary>
    /// Current working visitor in this propagator. On each call of <see cref="IPropagator{,}.Propagate"/>
    /// this visitor will be used to implement algorithm logic
    /// </summary>
    public IVisitor<TNode, TEdge> Visitor { get; protected set; }
    /// <summary>
    /// Current working graph
    /// </summary>
    /// <value></value>
    public IGraph<TNode, TEdge> Graph { get; protected set; }
    /// <summary>
    /// Default state for node.
    /// </summary>
    public const byte None = 0;
    /// <summary>
    /// In this state node is in check for visit in next iteration
    /// </summary>
    public const byte ToVisit = 1;
    /// <summary>
    /// In this state node is visited for current iteration
    /// </summary>
    public const byte Visited = 2;
    /// <summary>
    /// Represents an array of states of each node.<br/>
    /// Index to this array is node <paramref name="Id"/>, value is <see cref="byte"/> that contains all bit flags (states) 
    /// this node have.
    /// </summary>
    public byte[] NodeFlags => _nodeFlags;
    protected byte[] _nodeFlags;
    
    /// <param name="visitor">Visitor to use</param>
    /// <param name="graph">Graph to use</param>
    /// <param name="reverseOrder">You can apply propagator in straight order by iterating BFS on out edges of each node, or do reverse order iteration by doing BFS on in edges of each node</param>
    public PropagatorBase(IVisitor<TNode, TEdge> visitor, IGraph<TNode, TEdge> graph, bool reverseOrder = false)
    {
        ReverseOrder = reverseOrder;
        Visitor = visitor;
        Graph = graph;
        _nodeFlags = ArrayPoolStorage.ByteArrayPool.Rent(Graph.Nodes.MaxNodeId + 1);
        Array.Fill(_nodeFlags,(byte)0);
    }
    ~PropagatorBase(){
        ArrayPoolStorage.ByteArrayPool.Return(_nodeFlags);
    }
    public void SetPosition(params int[] nodeIndices)
    {
        int nodesCount = _nodeFlags.Length;
        Array.Clear(_nodeFlags, 0, _nodeFlags.Length);
        for (int i = 0; i < nodeIndices.Count(); i++)
        {
            _nodeFlags[nodeIndices[i] % nodesCount] |= Visited;
        }
    }
    public void Reset(IGraph<TNode, TEdge> graph, IVisitor<TNode,TEdge> visitor)
    {
        Graph = graph;
        Visitor = visitor;
        ReverseOrder = false;
        if(graph.Nodes.MaxNodeId+1<=_nodeFlags.Length){
            Array.Clear(_nodeFlags, 0, _nodeFlags.Length);
            return;
        }
        ArrayPoolStorage.ByteArrayPool.Return(_nodeFlags);

        _nodeFlags = ArrayPoolStorage.ByteArrayPool.Rent(Graph.Nodes.MaxNodeId + 1);
    }


    public bool IsNodeInState(int nodeId, byte state)
    {
        return (_nodeFlags[nodeId] & state) == state;
    }

    public void SetNodeState(int nodeId, byte state)
    {
        _nodeFlags[nodeId] |= state;
    }

    public void RemoveNodeState(int nodeId, byte state)
    {
        _nodeFlags[nodeId] &= (byte)~state;
    }
    /// <summary>
    /// Propagate nodes. In general it is just modifiable BFS.<br/>
    /// 1) Call <see cref="IVisitor{,}.Start"/> to prepare algorithm for next iteration <br/>
    /// 2) Find nodes that have state <see cref="PropagatorBase{,}.ToVisit"/>. <br/>
    /// 3) Apply <see cref="IVisitor{,}.Select"/> on each of out edges of this nodes. If edge is selected then it's target node state marked as <see cref="PropagatorBase{,}.Visited"/>. <br/>
    /// 5) Apply <see cref="IVisitor{,}.Visit"/> on each of nodes that have <see cref="PropagatorBase{,}.Visited"/> state. Remarkable that although many edges form step 3 can direct into the same node, this method will be called on each marked node exactly once. <br/>
    /// 6) For each node that have state <see cref="PropagatorBase{,}.Visited"/> remove that state from it and add instead <see cref="PropagatorBase{,}.ToVisit"/> state, so we can proceed next iteration. <br/>
    /// 7) Call <see cref="IVisitor{,}.End"/>
    /// </summary>
    public void Propagate()
    {
        Visitor.Start();
        PropagateNodes();
        Visitor.End();

        for (int i = 0; i < _nodeFlags.Length; i++)
        {
            if (IsNodeInState(i, Visited))
            {
                RemoveNodeState(i, Visited);
                SetNodeState(i, ToVisit);
            }
            else
                RemoveNodeState(i, ToVisit);
        }
    }
    /// <summary>
    /// This method implements how to call <see cref="PropagatorBase{,}.PropagateNode"/> on nodes.
    /// </summary>
    protected abstract void PropagateNodes();
    /// <summary>
    /// Iterates trough
    /// </summary>
    /// <param name="nodeId"></param>
    protected void PropagateNode(int nodeId)
    {
        if (ReverseOrder)
            foreach (var edge in Graph.Edges.InEdges(nodeId))
            {
                if (!Visitor.Select(edge)) continue;
                _nodeFlags.DangerousGetReferenceAt(edge.SourceId) |= Visited;
            }
        else
            foreach (var edge in Graph.Edges.OutEdges(nodeId))
            {
                if (!Visitor.Select(edge)) continue;
                _nodeFlags.DangerousGetReferenceAt(edge.TargetId) |= Visited;
            }
    }

    public byte GetNodeStates(int nodeId)
    {
        return _nodeFlags[nodeId];
    }

    public void ClearNodeStates(int nodeId)
    {
        _nodeFlags[nodeId] = None;
    }
}