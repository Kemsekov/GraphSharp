using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp
{
    public partial class NodesFactory
    {
        protected Random _rand;
        protected Func<int, INode> _createNode;
        protected Func<INode, INode, IEdge> _createEdge;
        public IList<INode> Nodes { get; protected set; }
        /// <summary>
        /// group of nodes that selected to be modified in next invocations.<br/>
        /// For example <see cref="NodesFactory.ForEach"/> will set this property just to <see cref="NodesFactory.Nodes"/> and
        /// next invocations of any operation will be performed on all nodes.
        /// <see cref="NodesFactory.ForOne"/> will set this property to just one particular node from <see cref="NodesFactory.Nodes"/>.
        /// <see cref="NodesFactory.ForNodes"/> will set this property to any subset of <see cref="NodesFactory.Nodes"/> 
        /// </summary>
        /// <value></value>
        public IEnumerable<INode> WorkingGroup { get; protected set; }

        /// <param name="createNode">Function to create nodes. Use it to CreateNodes for your own implementations of INode</param>
        /// <param name="createChild">Method that consists of node, parent and returns edge. createChild = (node,parent)=>new SomeNode(node,parent,...) // etc..</param>
        /// <param name="rand">Use your own rand if you need to get the same output per invoke. Let it null to use new random.</param>
        public NodesFactory(Func<int, INode> createNode = null, Func<INode, INode, IEdge> createChild = null, Random rand = null)
        {
            createNode ??= id => new Node(id);
            createChild ??= (node, parent) => new Edge(node);
            _rand = rand ?? new Random(); ;
            _createNode = createNode;
            _createEdge = createChild;
        }
        /// <summary>
        /// Replace current <see cref="NodesFactory.Nodes"/> to nodes
        /// </summary>
        /// <param name="nodes">What need to be used as Nodes</param>
        /// <returns></returns>
        public NodesFactory UseNodes(IList<INode> nodes)
        {
            Nodes = nodes;
            return ForEach();
        }
        /// <summary>
        /// Create count nodes. This method will replace existing Nodes in current instance of NodesFactory.
        /// </summary>
        /// <param name="count">Count of codes to create</param>
        /// <returns></returns>
        public NodesFactory CreateNodes(int count)
        {
            var nodes = new List<INode>(count);

            //create nodes
            for (int i = 0; i < count; i++)
            {
                var node = _createNode(i);
                nodes.Add(node);
            }
            return UseNodes(nodes);
        }

        /// <summary>
        /// Will set <see cref="NodesFactory.WorkingGroup"/> to <see cref="NodesFactory.Nodes"/>
        /// </summary>
        /// <returns></returns>
        public NodesFactory ForEach()
        {
            WorkingGroup = Nodes;
            return this;
        }

        /// <summary>
        /// Will set <see cref="NodesFactory.WorkingGroup"/> to particular node from <see cref="NodesFactory.Nodes"/> with id == nodeId
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public NodesFactory ForOne(int nodeId)
        {
            WorkingGroup = Nodes.Where(x => x.Id == nodeId);
            return this;
        }

        /// <summary>
        /// Will set <see cref="NodesFactory.WorkingGroup"/> to some subset of <see cref="NodesFactory.Nodes"/>
        /// </summary>
        /// <param name="selector">receive <see cref="NodesFactory.Nodes"/> and returns some set of values from them</param>
        /// <returns></returns>
        public NodesFactory ForNodes(Func<IList<INode>, IEnumerable<INode>> selector)
        {
            WorkingGroup = selector(Nodes);
            return this;
        }

        /// <summary>
        /// Randomly adds to node's Edges another nodes. It connect nodes to each other from current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        /// <param name="edgesCount">How much edges each node need</param>
        public NodesFactory ConnectNodes(int edgesCount)
        {
            edgesCount = edgesCount > Nodes.Count ? Nodes.Count : edgesCount;

            foreach (var node in WorkingGroup)
            {
                var start_index = _rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, start_index, edgesCount);
            }
            return this;
        }
        /// <summary>
        /// Randomly add to node's Edges another nodes, but create random count of connections. It connect nodes to each other from current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        /// <param name="minEdgesCount">Min count of edges of each node</param>
        /// <param name="maxEdgesCount">Max count of edges of each node</param>
        public NodesFactory ConnectRandomly(int minEdgesCount, int maxEdgesCount)
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
                int edgesCount = _rand.Next(maxEdgesCount - minEdgesCount) + minEdgesCount;
                var startIndex = _rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, startIndex, edgesCount);
            }
            return this;
        }

        /// <summary>
        /// Removes parent node from it's edges connection. Or simply makes any connection between nodes onedirectional in current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        public NodesFactory MakeDirected()
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
        /// ensures that every edge of parent's node have parent included in edges. Or simply make sure that any connection between two nodes are bidirectional in current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        public NodesFactory MakeUndirected()
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
                            edge.Node.Edges.Add(_createEdge(parent, edge.Node));
                        }
                     }
                 }
             });
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
                    node.Edges.Add(_createEdge(edge, node));

                }
        }
#nullable enable
        /// <summary>
        /// Randomly connects node to it's closest nodes by distance function in current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        /// <param name="minEdgesCount">minimum edges count</param>
        /// <param name="maxEdgesCount">maximum edges count</param>
        /// <param name="distance">Func to determine how much one node is distant from another</param>
        /// <returns></returns>
        public NodesFactory ConnectToClosest(int minEdgesCount, int maxEdgesCount, Func<INode, INode, double> distance)
        {
            Parallel.ForEach(WorkingGroup, parent =>
            {
                var edgesCount = _rand.Next(maxEdgesCount - minEdgesCount + 1) + minEdgesCount;
                var toAdd = ChooseClosestNodes(maxEdgesCount, edgesCount, distance, parent);
                foreach (var nodeId in toAdd)
                    parent.Edges.Add(_createEdge(Nodes[nodeId], parent));
            });
            return this;
        }
        IEnumerable<int> ChooseClosestNodes(int maxEdgesCount, int edgesCount, Func<INode, INode, double> distance, INode parent)
        {
            if (WorkingGroup.Count() == 0) return Enumerable.Empty<int>();

            var startNode = WorkingGroup.FirstOrDefault(x => x.Id != parent.Id);
            if (startNode is null) return Enumerable.Empty<int>();

            //front elements is smaller that back elements
            var buffer = new int[edgesCount];
            int size = 0;

            foreach (var el in WorkingGroup)
            {
                if (el.Id == parent.Id) continue;
                if (size != edgesCount)
                {
                    buffer[size++] = el.Id;
                    continue;
                }
                Array.Sort(buffer, (t1, t2) => (int)(100 * (distance(Nodes[t1], parent) - distance(Nodes[t2], parent))));

                if (distance(el, parent) < distance(Nodes[buffer[^1]], parent))
                {
                    //do circular-buffer push front
                    Buffer.BlockCopy(buffer, 0, buffer, 1 * sizeof(int), (size - 1) * sizeof(int));
                    buffer[0] = el.Id;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Removes all edges from it's parent in current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        /// <returns></returns>
        public NodesFactory RemoveAllConnections()
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