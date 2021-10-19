using GraphSharp.Vesitos;

namespace GraphSharp.Graphs
{
    public interface IGraph
    {
        void Clear();
        bool RemoveVesitor(IVesitor vesitor);
        void AddVesitor(IVesitor vesitor);
        void AddVesitor(IVesitor vesitor,params int[] nodes_id);
        void Step();
        void Step(IVesitor vesitor);

    }
}