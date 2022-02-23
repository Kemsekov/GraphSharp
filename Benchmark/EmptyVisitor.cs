using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

public class EmptyVisitor : Visitor<EmptyNode,EmptyEdge>
{
    public EmptyVisitor()
    {
        Propagator = new ParallelPropagator<EmptyNode>(this);
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