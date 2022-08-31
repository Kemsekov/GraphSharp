using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GraphSharp.Graphs;

using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Tests.Models
{
    public class TestVisitor : Visitor<Node, Edge>
    {
        
        public TestVisitor(IGraph<Node, Edge> graph)
        {
            this.Propagator = new ParallelPropagator<Node,Edge>(this,graph);
        }

        public override IPropagator<Node,Edge> Propagator{get;}

        public override void BeforeSelect()
        {
            
        }

        public override void EndVisit()
        {
        }

        public override bool Select(Edge edge)
        {
            return true;
        }
        public override void Visit(Node node)
        {

        }
    }
}