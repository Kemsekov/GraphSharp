using System;
using GraphSharp.Children;
using GraphSharp.Nodes;

namespace GraphSharp.Visitors
{
    /// <summary>
    /// All main logic of graph is contained here
    /// </summary>
    public interface IVisitor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <returns>Whatever this child must be visited or not</returns>
        bool Select(IChild child);
        /// <summary>
        /// Visit child. Note: this method will be called only once at particular node.
        /// </summary>
        /// <param name="child"></param>
        void Visit(IChild child);
        /// <summary>
        /// This method called right after visitor propagation is end.
        /// </summary>
        void EndVisit();
    }
}