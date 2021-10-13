using System.Collections.Generic;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;

namespace GraphSharp.Graphs
{
    public interface IGraph
    {
        void Clear();
        bool RemoveVesitor(IVesitor vesitor);
        void AddVesitor(IVesitor vesitor);
        void AddVesitor(IVesitor vesitor,int index);
        void Start();
        void Start(IVesitor vesitor);
        void Step();
        void Step(IVesitor vesitor);

    }
}