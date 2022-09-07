using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GraphSharp.Graphs;

using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Tests.Models
{
    public class TestVisitor : VisitorWithPropagator<Node, Edge>
    {
        
        public override IPropagator<Node, Edge> Propagator{get;}
        public TestVisitor(IGraph<Node, Edge> graph)
        {
            this.Propagator = new ParallelPropagator<Node,Edge>(this,graph);
        }
    }
}