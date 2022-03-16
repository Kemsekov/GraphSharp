using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;

namespace GraphSharp.Tests.Models
{
    public class TestVisitor : Visitor<TestNode, TestEdge>
    {
        
        public TestVisitor()
        {
            this.Propagator = new ParallelPropagator<TestNode,TestEdge>(this);
        }

        public override IPropagator<TestNode> Propagator{get;}

        public override void EndVisit()
        {
        }

        public override bool Select(TestEdge edge)
        {
            return true;
        }
        public override void Visit(TestNode node)
        {

        }
    }
}