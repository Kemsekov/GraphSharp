using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    public interface IGraphShared<IChild,TVisitor> where TVisitor : IVisitorShared<IChild>
    {
        /// <summary>
        /// Clears graph data. After this method is called you should add <see cref="IVisitor"/> again.
        /// This method does not clear nodes. They stay the same.
        /// </summary>
        void Clear();
        /// <summary>
        /// RemoVis Visitor from graph and all it's data. After this method is called <see cref="Step()"/> method will not call Visitor you removed
        /// </summary>
        /// <param name="Visitor">Visitor to be removed</param>
        /// <returns></returns>
        bool RemoveVisitor(TVisitor visitor);
        /// <summary>
        /// Add Visitor to graph. This Visitor will be called on each node that graph Visit from <see cref="Step()"/> method.
        /// </summary>
        /// <param name="Visitor">Visitor to add</param>
        void AddVisitor(TVisitor visitor);
        /// <summary>
        /// Add Visitor and assign it starting nodes to <see cref="nodes_id"/>
        /// </summary>
        /// <param name="Visitor">Visitor to add</param>
        /// <param name="nodes_id">Id's of nodes this Visitor must be assigned to</param>
        void AddVisitor(TVisitor visitor,params int[] nodes_id);
        /// <summary>
        /// Steps through nodes and move all Visitors to next node generation
        /// </summary>
        void Step();
        /// <summary>
        /// Steps through nodes for specified Visitor
        /// </summary>
        /// <param name="Visitor"></param>
        void Step(TVisitor visitor);
    }
}