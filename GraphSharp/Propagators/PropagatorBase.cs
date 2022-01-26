using System;
using System.Collections.Generic;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Propagators
{
    public abstract class PropagatorBase : IPropagator
    {
        protected INode[] _nodes;
        protected IVisitor _visitor;
        protected byte[] _visited;
        protected byte[] _toVisit;
        protected Action PropagateRun = null;

        /// <param name="indices">Starting nodes indices</param>
        public PropagatorBase(INode[] nodes, IVisitor visitor, params int[] indices)
        {
            _nodes = nodes;
            _visitor = visitor;
            _visited = new byte[_nodes.Length];
            _toVisit = new byte[_nodes.Length];
            AssignToNodes(visitor,indices);
        }
        public void AssignToNodes(IVisitor visitor,params int[] indices){
            Array.Clear(_visited,0,_visited.Length);
            Array.Clear(_toVisit,0,_toVisit.Length);
            var startNode = CreateStartingNode(indices);
            //first time we call Propagate we need to process starting Node.
            PropagateRun = () =>
            {
                PropagateNode(startNode);
                //later we need to let program run itself with visit cycle.
                PropagateRun = PropagateNodes;
            };
        }
        public virtual void Propagate()
        {
            // clear all states of visited for current nodes for next generation
            Array.Clear(_visited, 0, _visited.Length);

            PropagateRun();

            _visitor.EndVisit();

            //swap next generaton and current.
            var buf = _visited;
            _visited = _toVisit;
            _toVisit = buf;
        }
        /// <summary>
        /// create node with id = -1 and with edges that contain nodes with following indices
        /// </summary>
        /// <param name="indices"></param>
        /// <returns></returns>
        protected INode CreateStartingNode(params int[] indices)
        {
            var startNode = new Node(-1);
            foreach (var index in indices)
            {
                var child = new Edge(_nodes[index % _nodes.Length]);
                startNode.Edges.Add(child);
            }
            return startNode;
        }
        protected abstract void PropagateNode(INode node);
        protected abstract void PropagateNodes();
        
    }
}