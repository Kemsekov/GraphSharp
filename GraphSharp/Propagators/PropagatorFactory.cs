using System;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

namespace GraphSharp.Propagators
{
    /// <summary>
    /// Set of ready to use <see cref="IPropagator"/>s. Feel free to extend it with your own implementations of Propagator
    /// </summary>
    public partial class PropagatorFactory
    {
        /// <summary>
        /// delegate for creating some <see cref="IPropagator"/>.
        /// </summary>
        /// <param name="nodes">Nodes in a graph</param>
        /// <param name="visitor">Visitor associated with returning propagator</param>
        /// <param name="indices">Starting node indices for visitor</param>
        /// <returns></returns>
        public delegate IPropagator Factory(INode[] nodes, IVisitor visitor,params int[] indices);
        /// <summary>
        /// Returns factory for single threaded propagator. It means that Graph that uses this factory will handle every step in one thread only.
        /// This will allow you to get predictable behavior in every run of Graph Step function
        /// </summary>
        /// <returns></returns>
        public static Factory SingleThreaded()=> (nodes, visitor,indices) => new Propagator(nodes,visitor,indices);
        /// <summary>
        /// Returns factory for parallel propagator implementation. This implementation is a lot faster that <see cref="PropagatorFactory.SingleThreaded"/>, but 
        /// every call of <see cref="IPropagator.Propagate"/> will have different order, although results of each call will be the same as single threaded.
        /// BE WARY! When you use this version of <see cref="IPropagator"/> factory you'll have to implement following <see cref="IVisitor"/> parameter with concurency in mind.
        /// </summary>
        /// <returns></returns>
        public static Factory Parallel() => (nodes, visitor,indices) => new ParallelPropagator(nodes,visitor,indices);
    }
}