using System;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// feel free to extend it with your own implementations of Propagator
    /// </summary>
    public partial class PropagatorFactory
    {
        public delegate IPropagator Factory(INode[] nodes, IVisitor visitor,int[] indices);
        /// <summary>
        /// Returns factory for single threaded propagator. It means that Graph that uses this factory will handle every step in one thread only.
        /// This will allow you to get predictable behavior in every run of Graph Step function
        /// </summary>
        /// <returns></returns>
        public static Factory SingleThreaded()=> (nodes, visitor,indices) => new Propagator(nodes,visitor,indices);
        /// <summary>
        /// Returns factory for parallel propagator implementation. This implementation is a lot faster that single threaded, but 
        /// every Step of Graph will be have different order, although results of each step will be the same as single threaded.
        /// BE WARY! If you use parallel version with visitors than your visitor implementation must be thread-safe, elseware 
        /// execution of visitor logic can lead to undefined behavior. 
        /// </summary>
        /// <returns></returns>
        public static Factory Parallel() => (nodes, visitor,indices) => new ParallelPropagator(nodes,visitor,indices);
    }
}