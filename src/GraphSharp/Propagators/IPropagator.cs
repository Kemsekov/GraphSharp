namespace GraphSharp.Propagators
{
    public interface IPropagator
    {
        /// <summary>
        /// Move previous generation of nodes to next, visiting them in the process by some <see cref="GraphSharp.Visitors.IVisitor"/>.
        /// </summary>
        void Propagate();
    }
}