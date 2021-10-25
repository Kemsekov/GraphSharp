namespace GraphSharp.Visitors
{
    /// <summary>
    /// base interface for all visitors
    /// </summary>
    /// <typeparam name="TChild">Type of child of node</typeparam>
    public interface IVisitorShared<TChild> where TChild : IChild
    {
        /// <summary>
        /// End visit for node
        /// </summary>
        /// <param name="node">node to be end visited</param>
        void EndVisit();
        /// <summary>
        /// Method that selects which nodes need to be visited and which not
        /// </summary>
        /// <param name="node">Node to be selected</param>
        /// <returns>True - visit node. False - do not visit</returns>
        bool Select(TChild node);
    }
}