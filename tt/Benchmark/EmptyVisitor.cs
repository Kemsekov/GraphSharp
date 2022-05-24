using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

public class EmptyVisitor : Visitor<EmptyNode,EmptyEdge>
{
    public override IPropagator<EmptyNode,EmptyEdge> Propagator{get;}
    public EmptyVisitor(IGraphStructure<EmptyNode,EmptyEdge> graph)
    {
        Propagator = new ParallelPropagator<EmptyNode,EmptyEdge>(this,graph);
    }


    public override void EndVisit()
    {
    }

    public override bool Select(EmptyEdge edge)
    {
        return true;
    }

    public override void Visit(EmptyNode node)
    {
        
    }
}