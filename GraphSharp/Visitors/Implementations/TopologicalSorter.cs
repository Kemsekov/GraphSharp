using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GraphSharp.Common;
using GraphSharp.Graphs;
using GraphSharp.Propagators;
namespace GraphSharp.Visitors;

/// <summary>
/// Visitor that proceed topological sort for any graph.
/// </summary>
public class TopologicalSorter<TNode,TEdge> : VisitorWithPropagator<TNode, TEdge>
where TNode : INode
where TEdge : IEdge
{
    public override PropagatorBase<TNode, TEdge> Propagator { get; }
    ByteStatesHandler NodeStates => Propagator.NodeStates;
    /// <summary>
    /// After topological sort is done all nodes will be sorted out to different layers.
    /// Nodes on each layer have the same X coordinate and each following layer have X coordinate bigger that previous one.
    /// </summary>
    public IList<IList<TNode>> Layers { get; }
    public const byte Added = 16;
    /// <param name="graph">Algorithm will be executed on this graph</param>
    /// <param name="startingNodes">A set of nodes that will be used as start point for doing topological sort. If empty will be assigned to sources from a graph.</param>
    public TopologicalSorter(IGraph<TNode, TEdge> graph, params int[] startingNodes)
    {
        Propagator = new ParallelPropagator<TNode, TEdge>(this, graph);
        var startingNodesList = new List<int>(startingNodes);
        if(startingNodesList.Count==0)
        foreach(var n in graph.Nodes){
            if(graph.Edges.IsSource(n.Id))
                startingNodesList.Add(n.Id);
        }
        Layers = new List<IList<TNode>>();

        if(startingNodesList.Count==0)
            throw new ArgumentException("Cannot do topological sort because starting nodes were not given and there is no sources in a graph");
        var pos = startingNodesList.ToArray();
        Propagator.SetPosition(pos);
        foreach(var n in pos)
            NodeStates.AddState(Added,n);
        this.End();
    }
    protected override bool SelectImpl(TEdge edge)
    {
        
        if (NodeStates.IsInState(Added,edge.TargetId))
        {
            return false;
        }
        NodeStates.AddState(Added,edge.TargetId);
        return true;
    }
    protected override void VisitImpl(TNode node)
    {
        lock (Layers)
            Layers[^1].Add(node);
    }
    protected override void EndImpl()
    {
        if (Done) return;
        if (Layers.Count > 0)
        {
            if (Layers[^1].Count == 0)
            {
                this.Done = true;
                return;
            }
        }
        Layers.Add(new List<TNode>());
    }
    /// <summary>
    /// After all nodes have been sorted to different layers 
    /// this method will assign corresponding X coordinate to each layer.
    /// </summary>
    public void ApplyTopologicalSort(Action<TNode,Vector2> setPos)
    {
        if (!Done) return;
        float startNodePosition = 0f;
        var nodePositionShift = 1.0f / (Layers.Count() - 2);

        foreach (var layer in Layers)
        {
            var yStep = 1.0f/layer.Count;
            var y = 0f;
            foreach (var node in layer)
            {
                setPos(node,new(startNodePosition, y));
                y+=yStep;
            }
            startNodePosition += nodePositionShift;
        }
    }

    protected override void StartImpl()
    {
    }
}