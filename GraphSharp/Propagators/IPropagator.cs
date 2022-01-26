using GraphSharp.Visitors;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// Propagate trough nodes
    /// </summary>
    public interface IPropagator
    {
        /// <summary>
        /// Move previous generation of nodes to next, visiting them in the process by some <see cref="GraphSharp.Visitors.IVisitor"/>.
        /// </summary>
        void Propagate();
        /// <summary>
        /// Assign propagator to node indices.
        /// </summary>
        /// <param name="indices">Nodes</param>
        void AssignToNodes(params int[] indices);

    }
}