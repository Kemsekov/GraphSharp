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
        public IList<INode> Nodes { get; protected set; }
        /// <summary>
        /// group of nodes that selected to be modified in next invocations.<br/>
        /// For example nodesFactory.ForEach() will set this property just to Nodes and
        /// next invocations of any operation will be performed on all nodes.
        /// nodesFactory.ForOne(nodeId) will set this property to just one particular node from Nodes.
        /// nodeFactory.ForNodes(nodes=>nodes.Where(...)) will set this property to any set of nodes
        /// </summary>
        /// <value></value>
        public IEnumerable<INode> WorkingGroup { get; protected set; }

        /// <param name="createNode">Function to create nodes. Use it to CreateNodes for your own implementations of INode</param>
        /// <param name="createChild">Method that consists of node, parent and returns child. createChild = (node,parent)=>new SomeNode(node,parent,...) // etc..</param>
        /// <param name="rand">Use your own rand if you need to get the same output per invoke. Let it null to use new random.</param>
        public NodesFactory(Func<int, INode> createNode = null, Func<INode, INode, IChild> createChild = null, Random rand = null)
        {
            createNode ??= id => new Node(id);
            createChild ??= (node, parent) => new Child(node);
            _rand = rand ?? new Random(); ;
            _createNode = createNode;
            _createChild = createChild;
        }
        /// <summary>
        /// Replace Nodes in current instance of NodesFactory to nodes
        /// </summary>
        /// <param name="nodes">What need to be used as Nodes</param>
        /// <returns></returns>
        public NodesFactory UseNodes(IList<INode> nodes)
        {
            Nodes = nodes;
            return this;
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
            Nodes = nodes;
            return this;
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
        /// Will set <see cref="NodesFactory.WorkingGroup"/> to particular node from <see cref="NodesFactory.Nodes"/> with id = nodeId
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public NodesFactory ForOne(int nodeId){
            WorkingGroup = Nodes.Where(x=>x.Id==nodeId);
            return this;
        }

        /// <summary>
        /// Will set <see cref="NodesFactory.WorkingGroup"/> to some enumerable of nodes from that must be selected from <see cref="NodesFactory.Nodes"/>
        /// </summary>
        /// <param name="selector">receive <see cref="NodesFactory.Nodes"/> and returns some set of values from them</param>
        /// <returns></returns>
        public NodesFactory ForNodes(Func<IList<INode>,IEnumerable<INode>> selector){
            WorkingGroup = selector(Nodes);
            return this;
        }

        /// <summary>
        /// Randomly adds to node's Children another nodes. It connect nodes to each other from current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        /// <param name="countOfConnections">How much children node need</param>
        public NodesFactory ConnectNodes(int countOfConnections)
        {
            countOfConnections = countOfConnections > Nodes.Count ? Nodes.Count : countOfConnections;

            foreach (var node in WorkingGroup)
            {
                var start_index = _rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, start_index, countOfConnections);
            }
            return this;
        }
        /// <summary>
        /// Randomly add to node's Children another nodes, but create random count of connections. It connect nodes to each other from current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
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

            foreach (var node in WorkingGroup)
            {
                int count_of_connections = _rand.Next(maxCountOfNodes - minCountOfNodes) + minCountOfNodes;
                var start_index = _rand.Next(Nodes.Count);
                ConnectNodeToNodes(node, start_index, count_of_connections);
            }
            return this;
        }

        /// <summary>
        /// Removes parent node from it's children connection. Or simply makes any connection between nodes onedirectional in current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        public NodesFactory MakeDirected()
        {
            foreach (var parent in WorkingGroup)
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
        /// ensures that every child of parent's node have parent included in children. Or simply make sure that any connection between two nodes are bidirectional in current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        public NodesFactory MakeUndirected()
        {
            foreach (var parent in WorkingGroup)
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
        private void ConnectNodeToNodes(INode node, int start_index, int count_of_connections)
        {
            lock (node)
                for (int i = 0; i < count_of_connections; i++)
                {
                    var child = Nodes[(start_index + i) % Nodes.Count];
                    if (child.Id == node.Id)
                    {
                        start_index++;
                        i--;
                        continue;
                    }
                    node.Children.Add(_createChild(child, node));

                }
        }

        /// <summary>
        /// Randomly connects node to it's closest nodes by distance function in current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        /// <param name="minChildCount">minimum children count</param>
        /// <param name="maxChildCount">maximum children count</param>
        /// <param name="distance">Func to determine how much one node is distant from another</param>
        /// <returns></returns>
        public NodesFactory ConnectToClosest(int minChildCount, int maxChildCount, Func<INode, INode, double> distance)
        {
            foreach (var parent in WorkingGroup)
            {
                if (parent.Children.Count > maxChildCount) continue;
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
                            min = (pretendent, distance(parent, pretendent));
                            continue;
                        }
                        var pretendent_distance = distance(parent, pretendent);
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

        /// <summary>
        /// removes all children of parent nodes and every connection to parent node from any other node. It simply isolate node from others in current <see cref="NodesFactory.WorkingGroup"/>
        /// </summary>
        /// <returns></returns>
        public NodesFactory RemoveAllConnections()
        {
            foreach (var parent in WorkingGroup)
            {
                parent.Children.Clear();
                foreach (var node in Nodes)
                {
                    var toRemove = node.Children.FirstOrDefault(x => x.Node.Id == parent.Id);
                    if (toRemove is not null)
                        node.Children.Remove(toRemove);
                }
            }
            return this;
        }
    }
}