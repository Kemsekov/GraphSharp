using System.Collections.Generic;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Propagators
{
    public interface IPropagator<TNode>
    where TNode : INode
    {
        void Propagate();
        void SetPosition(params int[] nodeIndices);
        void SetNodes(IGraphStructure<TNode> nodes);
    }
}