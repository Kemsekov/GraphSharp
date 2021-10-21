using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    /// <summary>
    /// Interface for creating custom graphs.
    /// </summary>
    public interface IGraph<T> : IGraphShared<NodeValue<T>,IVisitor<T>>
    {
        
    }
}