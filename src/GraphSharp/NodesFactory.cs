using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Children;
using GraphSharp.Nodes;

namespace GraphSharp
{
    public partial class NodesFactory
    {
        protected Random _rand;
        protected Func<int, INode> _createNode;
        protected Func<INode, INode, IChild> _createChild;
        public IList<INode> Nodes{get;protected set;}

        /// <param name="createNode">Function to create nodes. Use it to CreateNodes for your own implementations of INode</param>
        /// <param name="createChild">Method that consists of node, parent and returns child. createChild = (node,parent)=>new SomeNode(node,parent,...) // etc..</param>
        /// <param name="rand">Use your own rand if you need to get the same output per invoke. Let it null to use new random.</param>
        public NodesFactory(Func<int, INode> createNode = null,Func<INode, INode, IChild> createChild = null, Random rand = null)
        {
            createNode ??= id => new Node(id);
            createChild ??= (node, parent) => new Child(node);
            _rand = rand ?? new Random();;
            _createNode = createNode;
            _createChild = createChild;
        }
        /// <summary>
        /// Create IList of INode's
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
            Nodes = nodes;
            return this;
        }
        /// <summary>
        /// Randomly add to each node's Children another nodes. It connect nodes to each other.
        /// </summary>
        /// <param name="nodes">Nodes to connect</param>
        /// <param name="countOfConnections">How much children each node need</param>
        public NodesFactory ConnectNodes(int countOfConnections)
        {
            countOfConnections = countOfConnections > Nodes.Count ? Nodes.Count : countOfConnections;

            foreach (var node in Nodes)
            {
                var start_index = _rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, Nodes, start_index, countOfConnections, _createChild);
            }
            return this;
        }
        /// <summary>
        /// Randomly add to each node's Children another nodes, but create random count of connections per node. It connect nodes to each other.
        /// </summary>
        /// <param name="nodes">Nodes to connect</param>
        /// <param name="minCountOfNodes">Min count of children of each node</param>
        /// <param name="maxCountOfNodes">Max count of children of each node</param>
        public NodesFactory ConnectRandomly(int minCountOfNodes, int maxCountOfNodes)
        {
            minCountOfNodes = minCountOfNodes < 1 ? 1 : minCountOfNodes;
            maxCountOfNodes = maxCountOfNodes > Nodes.Count ? Nodes.Count : maxCountOfNodes;

            //swap using xor
            if (minCountOfNodes > maxCountOfNodes)
            {
                minCountOfNodes = minCountOfNodes ^ maxCountOfNodes;
                maxCountOfNodes = minCountOfNodes ^ maxCountOfNodes;
                minCountOfNodes = minCountOfNodes ^ maxCountOfNodes;
            }

            foreach (var node in Nodes)
            {
                int count_of_connections = _rand.Next(maxCountOfNodes - minCountOfNodes) + minCountOfNodes;
                var start_index = _rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, Nodes, start_index, count_of_connections, _createChild);
            }
            return this;
        }

        /// <summary>
        /// Removes parent's node from it's children connection. Or simply makes any connection between nodes onedirectional.
        /// </summary>
        /// <param name="nodes"></param>
        public NodesFactory MakeDirected()
        {
            foreach (var parent in Nodes)
            {
                bool fine = false;
                while (!fine)
                {
                    fine = true;
                    for (int i = 0; i < parent.Children.Count; i++)
                    {
                        var child = parent.Children[i];

                        var toRemove = child.Node.Children.Any(x => x.Node.Id == parent.Id);
                        if (toRemove)
                        {
                            parent.Children.Remove(child);
                            fine = false;
                        }
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// ensures that every child of each node in nodes collection have it's parent included in children. Or simply make sure that any connection between two nodes are bidirectional. 
        /// </summary>
        /// <param name="nodes"></param>
        public NodesFactory MakeUndirected()
        {
            foreach (var parent in Nodes)
            {
                for (int i = 0; i < parent.Children.Count; i++)
                {
                    var child = parent.Children[i];

                    var toAdd = !child.Node.Children.Any(x => x.Node.Id == parent.Id);
                    if (toAdd)
                    {
                        child.Node.Children.Add(_createChild(parent, child.Node));
                    }
                }
            }
            return this;
        }

        //connect some node to List of nodes with some parameters.
        private static void ConnectNodeToNodes(INode node, IList<INode> nodes, int start_index, int count_of_connections, Func<INode, INode, IChild> createChild)
        {
            lock (node)
                for (int i = 0; i < count_of_connections; i++)
                {
                    var child = nodes[(start_index + i) % nodes.Count];
                    if (child.Id == node.Id)
                    {
                        start_index++;
                        i--;
                        continue;
                    }
                    node.Children.Add(createChild(child, node));

                }
        }

        /// <summary>
        /// Randomly connects current nodes to it's closest nodes by. How one node close to other defines in distance
        /// </summary>
        /// <param name="minChildCount"></param>
        /// <param name="maxChildCount"></param>
        /// <param name="distance">Func to determine how much one node is distant from another</param>
        /// <returns></returns>
        public NodesFactory ConnectToClosest(int minChildCount, int maxChildCount,Func<INode,INode,double> distance)
        {
            foreach (var parent in Nodes)
            {
                var childCount = _rand.Next(maxChildCount - minChildCount) + minChildCount;
                for (int i = 0; i < childCount; i++)
                {
                    (INode? node, double distance) min = (null, 0);
                    int shift = _rand.Next(Nodes.Count);

                    for (int b = 0; b < Nodes.Count; b++)
                    {
                        var pretendent = Nodes[(b + shift) % Nodes.Count];

                        if (pretendent.Id == parent.Id) continue;
                        if (pretendent.Children.Count > maxChildCount) continue;

                        if (min.node is null)
                        {
                            min = (pretendent, distance(parent,pretendent));
                            continue;
                        }
                        var pretendent_distance = distance(parent,pretendent);
                        if (pretendent_distance < min.distance && parent.Children.FirstOrDefault(x => x.Node.Id == pretendent.Id) is null)
                        {
                            min = (pretendent, pretendent_distance);
                        }
                    }
                    var node = min.node;
                    if (node is null) continue;
                    parent.Children.Add(_createChild(node, parent));
                    node.Children.Add(_createChild(parent, node));
                }
            }
            return this;
        }
    }
}