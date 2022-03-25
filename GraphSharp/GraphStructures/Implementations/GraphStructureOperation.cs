using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Extensions;
using GraphSharp.Nodes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.GraphStructures
{
    /// <summary>
    /// Contains methods to modify relationships between nodes and contain converters for graph structure.
    /// </summary>
    public class GraphStructureOperation<TNode,TEdge> : GraphStructureBase<TNode,TEdge>
    where TNode : NodeBase<TEdge>
    where TEdge : EdgeBase<TNode>
    {
        public GraphStructureOperation(GraphStructureBase<TNode, TEdge> graphStructure) : base(graphStructure)
        {
            Nodes = graphStructure.Nodes;
            WorkingGroup = graphStructure.WorkingGroup;
        }

        /// <summary>
        /// Randomly create edgesCount edges for each node from <see cref="IGraphStructure{}.WorkingGroup"/>
        /// </summary>
        /// <param name="edgesCount">How much edges each node need</param>
        public GraphStructureOperation<TNode,TEdge> ConnectNodes(int edgesCount)
        {
            edgesCount = edgesCount > Nodes.Count ? Nodes.Count : edgesCount;

            foreach (var node in WorkingGroup)
            {
                var start_index = Rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, start_index, edgesCount);
            }
            return this;
        }
        /// <summary>
        /// Randomly create some range of edges for each node from <see cref="IGraphStructure{}.WorkingGroup"/>
        /// </summary>
        /// <param name="minEdgesCount">Min count of edges for each node</param>
        /// <param name="maxEdgesCount">Max count of edges for each node</param>
        public GraphStructureOperation<TNode,TEdge> ConnectRandomly(int minEdgesCount, int maxEdgesCount)
        {
            minEdgesCount = minEdgesCount < 1 ? 1 : minEdgesCount;
            maxEdgesCount = maxEdgesCount > Nodes.Count ? Nodes.Count : maxEdgesCount;

            //swap using xor
            if (minEdgesCount > maxEdgesCount)
            {
                minEdgesCount = minEdgesCount ^ maxEdgesCount;
                maxEdgesCount = minEdgesCount ^ maxEdgesCount;
                minEdgesCount = minEdgesCount ^ maxEdgesCount;
            }

            foreach (var node in WorkingGroup)
            {
                int edgesCount = Rand.Next(maxEdgesCount - minEdgesCount) + minEdgesCount;
                var startIndex = Rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, startIndex, edgesCount);
            }
            return this;
        }
        
        /// <summary>
        /// Connect some node to edgesCount of other nodes
        /// </summary>
        private void ConnectNodeToNodes(TNode node, int startIndex, int edgesCount)
        {
            lock (node)
                for (int i = 0; i < edgesCount; i++)
                {
                    var edge = Nodes[(startIndex + i) % Nodes.Count];
                    if (edge.Id == node.Id)
                    {
                        startIndex++;
                        i--;
                        continue;
                    }
                    node.Edges.Add(CreateEdge(node,edge));

                }
        }

        /// <summary>
        /// Randomly connects closest nodes in current <see cref="IGraphStructure{}.WorkingGroup"/> using <see cref="GraphStructureBase{,}.Distance"/>. <br/> minEdgesCount and maxEdgesCount not gonna give 100% right results. This params are just approximation of how much edges per node is gonna be created.
        /// </summary>
        /// <param name="minEdgesCount">Approximately minimum edges count</param>
        /// <param name="maxEdgesCount">Approximately maximum edges count</param>
        public GraphStructureOperation<TNode,TEdge> ConnectToClosest(int minEdgesCount, int maxEdgesCount)
        {
            var edgesCountMap = new ConcurrentDictionary<INode, int>();
            foreach (var parent in WorkingGroup)
                edgesCountMap[parent] = Rand.Next(minEdgesCount, maxEdgesCount);

            var locker = new object();
            Parallel.ForEach(WorkingGroup, parent =>
            {
                var edgesCount = edgesCountMap[parent];
                if (parent.Edges.Count() >= edgesCount) return;
                var toAdd = ChooseClosestNodes(maxEdgesCount, maxEdgesCount, parent);
                foreach (var nodeId in toAdd)
                    lock (locker)
                    {
                        var node = Nodes[nodeId];
                        if (parent.Edges.Count() >= maxEdgesCount) return;
                        if (node.Edges.Count >= maxEdgesCount) continue;
                        parent.Edges.Add(CreateEdge(parent,node));
                        node.Edges.Add(CreateEdge(node,parent));
                    }
            });
            return this;
        }
        IEnumerable<int> ChooseClosestNodes(int maxEdgesCount, int edgesCount, TNode parent)
        {
            if (WorkingGroup.Count() == 0) return Enumerable.Empty<int>();

            var result = WorkingGroup.Select(x => x.Id).FindFirstNMinimalElements(
                n: edgesCount,
                comparison: (t1, t2) => Distance(parent, Nodes[t1]) > Distance(parent, Nodes[t2]) ? 1 : -1,
                skipElement: (nodeId) => Nodes[nodeId].Id == parent.Id || Nodes[nodeId].Edges.Count >= maxEdgesCount);

            return result;
        }
        /// <summary>
        /// Makes every connection between two nodes from <see cref="IGraphStructure{}.WorkingGroup"/> onedirectional.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> MakeDirected()
        {
            foreach (var parent in WorkingGroup)
            {
                bool fine = false;
                while (!fine)
                {
                    fine = true;
                    for (int i = 0; i < parent.Edges.Count; i++)
                    {
                        var edge = parent.Edges[i];

                        var toRemove = edge.Node.Edges.Any(x => x.Node.Id == parent.Id);
                        if (toRemove)
                        {
                            parent.Edges.Remove(edge);
                            fine = false;
                        }
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// Makes every connection between two nodes from <see cref="IGraphStructure{}.WorkingGroup"/> bidirectional.
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> MakeUndirected()
        {
            Parallel.ForEach(WorkingGroup, parent =>
             {

                 for (int i = 0; i < parent.Edges.Count; i++)
                 {
                     var edge = parent.Edges[i];

                     lock (edge.Node)
                     {
                        var toAdd = !edge.Node.Edges.Any(x => x.Node.Id == parent.Id);
                        if (toAdd)
                        {
                            edge.Node.Edges.Add(CreateEdge(edge.Node,parent));
                        }
                     }
                 }
             });
            return this;
        }
        /// <summary>
        /// Removes all edges from nodes from <see cref="IGraphStructure{}.WorkingGroup"/>
        /// </summary>
        public GraphStructureOperation<TNode,TEdge> RemoveAllEdges()
        {
            foreach (var parent in WorkingGroup)
            {
                parent.Edges.Clear();
                foreach (var node in Nodes)
                {
                    var toRemove = node.Edges.FirstOrDefault(x => x.Node.Id == parent.Id);
                    if (toRemove is not null)
                        node.Edges.Remove(toRemove);
                }
            }
            return this;
        }
        /// <summary>
        /// Clears current <see cref="IGraphStructure{}.WorkingGroup"/> 
        /// </summary>
        /// <returns><see cref="GraphStructure{,}"/> that can be used to reset <see cref="IGraphStructure{}.WorkingGroup"/> </returns>
        public GraphStructure<TNode,TEdge> ClearWorkingGroup(){
            this.WorkingGroup = Enumerable.Empty<TNode>();
            return new(this);
        }
    }
}