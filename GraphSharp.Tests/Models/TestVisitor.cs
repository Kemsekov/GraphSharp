using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;
using GraphSharp.Graphs;

using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Tests.Models
{
    public class TestVisitor : VisitorWithPropagator<Node, Edge>
    {
        
        public override PropagatorBase<Node, Edge> Propagator{get;}
        public TestVisitor(IGraph<Node, Edge> graph)
        {
            this.Propagator = new ParallelPropagator<Node,Edge>(this,graph);
        }

        protected override void StartImpl()
        {
        }

        protected override bool SelectImpl(EdgeSelect<Edge> edge)
        {
            return true;
        }

        protected override void VisitImpl(Node node)
        {
        }

        protected override void EndImpl()
        {
        }
    }
}