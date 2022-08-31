using System;
using GraphSharp.Visitors;
using System.Linq;
using GraphSharp.Graphs;
using Microsoft.Toolkit.HighPerformance;
namespace GraphSharp.Propagators;

/// <summary>
/// Base implementation class for <see cref="IPropagator{,}"/>.<br/>
/// By default this implementation assign a state to each node as byte with value of power of 2. 
/// There is 3 states that already used: None = 0, ToVisit = 1, Visited = 2.<br/>
/// So there is only 6 states left for your disposal: 4 8 16 32 64 128.<br/>
/// WARNING: if you using to assign or to check states(bytes) which values is not power of 2
/// then you may get unexpected behavior.
/// </summary>
public abstract class PropagatorBase<TNode, TEdge> : IPropagator<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    /// <summary>
    /// Current working visitor in this propagator. On each call of <see cref="IPropagator{,}.Propagate"/>
    /// this visitor will be used to implement algorithm logic
    /// </summary>
    public IVisitor<TNode, TEdge> Visitor { get; init; }
    /// <summary>
    /// Current working graph
    /// </summary>
    /// <value></value>
    public IGraph<TNode, TEdge> Graph {get;protected set;}
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
    protected byte[] _nodeFlags;
    /// <param name="visitor">Visitor to use</param>
    /// <param name="graph">Graph to use</param>
    public PropagatorBase(IVisitor<TNode, TEdge> visitor, IGraph<TNode, TEdge> graph)
    {
        Visitor = visitor;
        Graph = graph;
        _nodeFlags = new byte[Graph.Nodes.MaxNodeId + 1];
    }
    public void SetPosition(params int[] nodeIndices)
    {
        int nodesCount = Graph.Nodes.MaxNodeId + 1;
        Array.Clear(_nodeFlags, 0, _nodeFlags.Length);
        for (int i = 0; i < nodeIndices.Count(); i++)
        {
            _nodeFlags[nodeIndices[i] % nodesCount] |= Visited;
        }
    }
    public void SetGraph(IGraph<TNode, TEdge> graph)
    {
        Graph = graph;
        _nodeFlags = new byte[Graph.Nodes.MaxNodeId + 1];
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
    // TODO: add test that checks right order of visitor's methods execution
    
    /// <summary>
    /// Propagate nodes. In general it is just modifiable BFS.<br/>
    /// 1) Call <see cref="IVisitor{,}.BeforeSelect"/> to prepare algorithm for next iteration <br/>
    /// 2) Find nodes that have state <see cref="PropagatorBase{,}.ToVisit"/>. <br/>
    /// 3) Apply <see cref="IVisitor{,}.Select"/> on each of out edges of this nodes. If edge is selected then it's target node state marked as <see cref="PropagatorBase{,}.Visited"/>. <br/>
    /// 5) Apply <see cref="IVisitor{,}.Visit"/> on each of nodes that have <see cref="PropagatorBase{,}.Visited"/> state. Remarkable that although many edges form step 3 can direct into the same node, this method will be called on each marked node exactly once. <br/>
    /// 6) For each node that have state <see cref="PropagatorBase{,}.Visited"/> remove that state from it and add instead <see cref="PropagatorBase{,}.ToVisit"/> state, so we can proceed next iteration.
    /// </summary>
    public void Propagate()
    {
        Visitor.BeforeSelect();
        PropagateNodes();
        Visitor.EndVisit();

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
    protected void PropagateNode(int nodeId)
    {
        var edges = Graph.Edges.OutEdges(nodeId);
        foreach (var edge in edges)
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