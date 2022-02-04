using System.Collections.Generic;
using System.Linq;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
using Xunit;

namespace GraphSharp.Tests
{
    public class PropagatorTests{
        PropagatorFactory.Factory _propagatorFactory;
        private INode[] _nodes;

        public PropagatorTests()
        {
            _propagatorFactory = PropagatorFactory.SingleThreaded();
            _nodes = new GraphStructure().CreateNodes(1000).ForEach().ConnectNodes(10).Nodes.ToArray();
        }
        [Fact]
        public void AssignToNodes_Works()
        {
            var visitedNodes = new List<INode>();
            var visitor = new ActionVisitor(
                (node)=>
                {
                    lock(visitedNodes)
                        visitedNodes.Add(node);
                },
                (edge)=>true,
                ()=>visitedNodes.Sort());
            var propagator = _propagatorFactory.Invoke(_nodes,visitor,new[]{1,2});
            
            propagator.Propagate();
            Assert.Equal(visitedNodes,new[]{_nodes[1],_nodes[2]});
            visitedNodes.Clear();
            propagator.AssignToNodes(5,6);
            
            propagator.Propagate();
            Assert.Equal(visitedNodes,new[]{_nodes[5],_nodes[6]});
            visitedNodes.Clear();

        }
        
    }
}