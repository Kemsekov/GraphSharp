using System.Collections.Generic;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;

namespace GraphSharp.Graphs
{
    public interface IGraph
    {
        bool AddNode(NodeBase node);
        bool RemoveNode(NodeBase node);
        void AddNodes(IEnumerable<NodeBase> node);
        void Clear();
        void AddVesitor(IVesitor vesitor);
        void AddVesitor(IVesitor vesitor,int index);
        void Start();
        void Start(IVesitor vesitor);
        void Step();
        void Step(IVesitor vesitor);


    }
}