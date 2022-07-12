using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
namespace GraphSharp.Visitors;
public class TopologicalSorter<TNode,TEdge> : Visitor<TNode, TEdge>
where TNode : INode
where TEdge : IEdge<TNode>
{
    public override IPropagator<TNode, TEdge> Propagator { get; }
    public IList<IList<TNode>> Layers { get; }
    public bool Done { get; private set; } = false;
    public const byte Added = 4;
    public TopologicalSorter(GraphStructure<TNode, TEdge> graph)
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