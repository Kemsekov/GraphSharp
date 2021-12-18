using System.Collections.Generic;
using GraphSharp.Graphs.Propagators;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    public class ParallelGraph : Graph
    {
        public ParallelGraph(IEnumerable<INode> nodes) : base(nodes)
        {

        }

        public override void AddVisitor(IVisitor visitor, params int[] indices)
        {
            //this code is pretty much copy of Graph one, except it works concurrently.
            if (_work.ContainsKey(visitor)) return;
            var temp = new ParallelPropagator(_nodes,visitor,indices);
            _work.Add(visitor, temp);

        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}