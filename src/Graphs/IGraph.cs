using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Interface for creating custom graphs.
    /// </summary>
    public interface IGraph : IGraphShared<NodeBase,IVisitor>
    {
        
    }
}