namespace GraphSharp.Visitors
{
    public interface IVisitorShared<TChild>
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
        /// <returns>True - visit node. False - not visit node</returns>
        bool Select(TChild node);
    }
}