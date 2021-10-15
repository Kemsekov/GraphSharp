using GraphSharp.Nodes;

namespace GraphSharp.Vesitos
{
    public interface IVesitor
    {
        void Vesit(NodeBase node);
        void EndVesit(NodeBase node);
        /// <summary>
        /// Method that selects which nodes need to be vesited and which not
        /// </summary>
        /// <param name="node">Node to be selected</param>
        /// <returns></returns>
        bool Select(NodeBase node);
    }
}