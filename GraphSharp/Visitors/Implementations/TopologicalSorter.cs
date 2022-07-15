using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Graphs;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
namespace GraphSharp.Visitors;
/// <summary>
/// Visitor that proceed topological sort for any graph.
/// </summary>
public class TopologicalSorter<TNode,TEdge> : Visitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge<TNode>
{
    public override IPropagator<TNode, TEdge> Propagator { get; }
    /// <summary>
    /// After topological sort is done all nodes will be sorted out to different layers.
    /// Nodes on each layer have the same X coordinate and each following layer have X coordinate bigger that previous one.
    /// </summary>
    public IList<IList<TNode>> Layers { get; }
    /// <summary>
    /// True when topological sort is done
    /// </summary>
    public bool Done { get; private set; } = false;
    public const byte Added = 4;
    public TopologicalSorter(Graph<TNode, TEdge> graph)
    {
        Propagator = new ParallelPropagator<TNode, TEdge>(this, graph);
        var startingNodes = new List<int>();
        foreach(var n in graph.Nodes){
            if(graph.Edges.GetSourcesId(n.Id).Count()==0)
                startingNodes.Add(n.Id);
        }
        Layers = new List<IList<TNode>>();
        Propagator.SetPosition(startingNodes.ToArray());
        this.EndVisit();
    }

    public override void EndVisit()
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

    public override bool Select(TEdge edge)
    {
        if (Done) return false;
        if (Propagator.IsNodeInState(edge.Target.Id, Added))
        {
            return false;
        }
        Propagator.SetNodeState(edge.Target.Id, Added);
        return true;
    }

    public override void Visit(TNode node)
    {
        if (Done) return;
        lock (Layers)
            Layers[^1].Add(node);
    }
    /// <summary>
    /// After all nodes have been sorted to different layers 
    /// this method will assign corresponding X coordinate to each layer.
    /// </summary>
    public void DoTopologicalSort()
    {
        if (!Done) return;
        float startNodePosition = 0f;
        var nodePositionShift = 1.0f / (Layers.Count() - 2);

        foreach (var layer in Layers)
        {
            foreach (var node in layer)
            {
                node.Position = new(startNodePosition, node.Position.Y);
            }
            startNodePosition += nodePositionShift;
        }
    }
}