namespace GraphSharp.Propagators
{
    /// <summary>
    /// Logic of how should any graph move from previous generation of node to next, and what to do with them in the proccess.
    /// </summary>
    public interface IPropagator
    {
        /// <summary>
        /// Move previous generation of nodes to next, visiting them in the process by some <see cref="GraphSharp.Visitors.IVisitor"/>.
        /// </summary>
        void Propagate();
    }
}