using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GraphSharp.GraphStructures
{
    public class GraphStructureOperation : GraphStructureBase
    {
        public GraphStructureOperation(GraphStructureBase structureBase) : base(structureBase.CreateNode, structureBase.CreateEdge, structureBase.Rand)
        {
            Nodes = structureBase.Nodes;
            WorkingGroup = structureBase.WorkingGroup;
        }

        /// <summary>
        /// Randomly adds to node's Edges another nodes. It connect nodes to each other from current <see cref="GraphStructure.WorkingGroup"/>
        /// </summary>
        /// <param name="edgesCount">How much edges each node need</param>
        public GraphStructureOperation ConnectNodes(int edgesCount)
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
        /// Randomly add to node's Edges another nodes, but create random count of connections. It connect nodes to each other from current <see cref="GraphStructure.WorkingGroup"/>
        /// </summary>
        /// <param name="minEdgesCount">Min count of edges of each node</param>
        /// <param name="maxEdgesCount">Max count of edges of each node</param>
        public GraphStructureOperation ConnectRandomly(int minEdgesCount, int maxEdgesCount)
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



        //connect some node to List of nodes with some parameters.
        private void ConnectNodeToNodes(INode node, int startIndex, int edgesCount)
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
                    node.Edges.Add(CreateEdge(edge, node));

                }
        }

#nullable enable
        /// <summary>
        /// Randomly connects node to it's closest nodes by distance function in current <see cref="GraphStructure.WorkingGroup"/>
        /// </summary>
        /// <param name="minEdgesCount">minimum edges count</param>
        /// <param name="maxEdgesCount">maximum edges count</param>
        /// <param name="distance">Func to determine how much one node is distant from another</param>
        /// <returns></returns>
        public GraphStructureOperation ConnectToClosest(int minEdgesCount, int maxEdgesCount, Func<INode, INode, float> distance)
        {
            var edgesCountMap = new ConcurrentDictionary<INode, int>();
            foreach (var parent in WorkingGroup)
                edgesCountMap[parent] = Rand.Next(minEdgesCount, maxEdgesCount);

            var locker = new object();
            Parallel.ForEach(WorkingGroup, parent =>
             {
                 var edgesCount = edgesCountMap[parent];
                 if (parent.Edges.Count() >= edgesCount) return;
                 var toAdd = ChooseClosestNodes(maxEdgesCount, maxEdgesCount, distance, parent);
                 foreach (var nodeId in toAdd)
                     lock (locker)
                     {
                         var node = Nodes[nodeId];
                         if (parent.Edges.Count() >= maxEdgesCount) return;
                         if (node.Edges.Count >= maxEdgesCount) continue;
                         parent.Edges.Add(CreateEdge(node, parent));
                         node.Edges.Add(CreateEdge(parent, node));
                     }
             });
            return this;
        }
        /// <summary>
        /// Converts current <see cref="GraphStructureFactory.Nodes"/> to adjacency matrix
        /// </summary>
        /// <param name="calculateWeightFromEdge">By default any releationship in adjacency matrix is 1 if there is connection between nodes and 0 if there is no one. You can replace this numbers with weights calculated from edge with this <see cref="Func{IEdge,float}"/></param>
        /// <returns></returns>
        public Matrix ToAdjacencyMatrix(Func<IEdge, float>? calculateWeightFromEdge = null)
        {
            calculateWeightFromEdge ??= edge => 1;
            Matrix adjacencyMatrix;

            //if matrix size will be bigger than 64 mb place store it as sparse.
            if (Nodes.Count > 4096)
                adjacencyMatrix = SparseMatrix.Create(Nodes.Count, Nodes.Count, 0);
            else
                adjacencyMatrix = DenseMatrix.Create(Nodes.Count, Nodes.Count, 0);

            for (int i = 0; i < Nodes.Count; i++)
            {
                foreach (var e in Nodes[i].Edges)
                {
                    adjacencyMatrix[i, e.Node.Id] = calculateWeightFromEdge(e);
                }
            }
            return adjacencyMatrix;
        }
        IEnumerable<int> ChooseClosestNodes(int maxEdgesCount, int edgesCount, Func<INode, INode, float> distance, INode parent)
        {
            if (WorkingGroup.Count() == 0) return Enumerable.Empty<int>();

            var result = Helpers.Helpers.FindFirstNMinimalElements(
                n: edgesCount,
                src: WorkingGroup.Select(x => x.Id),
                comparison: (t1, t2) => distance(parent, Nodes[t1]) > distance(parent, Nodes[t2]) ? 1 : -1,
                skipElement: (nodeId) => Nodes[nodeId].Id == parent.Id || Nodes[nodeId].Edges.Count >= maxEdgesCount);

            return result;
        }
        /// <summary>
        /// Removes parent node from it's edges connection. Or simply makes any connection between nodes onedirectional in current <see cref="GraphStructureFactory.WorkingGroup"/>
        /// </summary>
        public GraphStructureOperation MakeDirected()
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
        /// ensures that every edge of parent's node have parent included in edges. Or simply make sure that any connection between two nodes are bidirectional in current <see cref="GraphStructureFactory.WorkingGroup"/>
        /// </summary>
        public GraphStructureOperation MakeUndirected()
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
                            edge.Node.Edges.Add(CreateEdge(parent, edge.Node));
                        }
                     }
                 }
             });
            return this;
        }
        /// <summary>
        /// Removes all edges from it's parent in current <see cref="GraphStructureFactory.WorkingGroup"/>
        /// </summary>
        /// <returns></returns>
        public GraphStructureOperation RemoveAllConnections()
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

    }
}