using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Graphs
{
    public interface IGraphShared<TChild,TVisitor> 
    where TVisitor : IVisitorShared<TChild>
    where TChild : IChild
    {
        /// <summary>
        /// Clears graph data. After this method is called you should add <see cref="IVisitor"/> again.
        /// This method does not clear nodes. They stay the same.
        /// </summary>
        void Clear();
        /// <summary>
        /// Removes visitor from graph and cleans all it's data. After this method is called <see cref="Step()"/> method will not call visitor you removed and will throw if you try to specify it directly in <see cref="Step(TVisitor)"/>
        /// </summary>
        /// <param name="Visitor">Visitor to be removed</param>
        /// <returns></returns>
        bool RemoveVisitor(TVisitor visitor);
        /// <summary>
        /// Adds visitor to graph. This visitor will be called on each node that graph Visit from <see cref="Step()"/> method.
        /// </summary>
        /// <param name="Visitor">Visitor to add</param>
        void AddVisitor(TVisitor visitor);
        /// <summary>
        /// Add Visitor with some starting nodes
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