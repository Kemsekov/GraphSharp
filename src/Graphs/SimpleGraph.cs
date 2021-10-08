using System.Collections.Generic;
using System.Linq;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;

namespace GraphSharp.Graphs
{
    public class SimpleGraph : IGraph
    {
        protected List<NodeBase> _nodes { get; }
        public SimpleGraph() : this(new List<NodeBase>())
        {
            
        }
        public SimpleGraph(IEnumerable<NodeBase> nodes)
        {
            _nodes = new List<NodeBase>(nodes);
        }   
        public bool AddNode(NodeBase node)
        {
            if (_nodes.Contains(node)) return false;
            _nodes.Add(node);
            return true;
        }
        public bool RemoveNode(NodeBase node)
        {
            if (_nodes.Remove(node))
            {
                return true;
            }
            return false;
        }
        public void AddNodes(IEnumerable<NodeBase> nodes)
        {
            var toAdd = nodes.Except(_nodes);
            _nodes.AddRange(toAdd);
        }

        public void AddVesitor(IVesitor vesitor)
        {
            throw new System.NotImplementedException();
        }

        public void AddVesitor(IVesitor vesitor, int index)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }

        public void Start(IVesitor vesitor)
        {
            throw new System.NotImplementedException();
        }

        public void Step()
        {
            throw new System.NotImplementedException();
        }

        public void Step(IVesitor vesitor)
        {
            throw new System.NotImplementedException();
        }
    }
}