using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Children;
using GraphSharp.Nodes;

namespace GraphSharp
{
    public static partial class NodeGraphFactory
    {
        /// <summary>
        /// Create IList of INode's
        /// </summary>
        /// <param name="count">Count of codes to create</param>
        /// <param name="createNode">Function to create nodes. Use it to CreateNodes for your own implementations of INode</param>
        /// <returns></returns>
        public static IList<INode> CreateNodes(int count, Func<int, INode> createNode = null)
        {
            createNode ??= id => new Node(id);

            var nodes = new List<INode>(count);

            //create nodes
            for (int i = 0; i < count; i++)
            {
                var node = createNode(i);
                nodes.Add(node);
            }
            return nodes;
        }
        /// <summary>
        /// Randomly add to each node's Children another nodes. It connect nodes to each other.
        /// </summary>
        /// <param name="nodes">Nodes to connect</param>
        /// <param name="countOfConnections">How much children each node need</param>
        /// <param name="rand">Use your own rand if you need to get the same output per invoke. Let it null to use new random.</param>
        /// <param name="createChild">Method that consists of node, parent and returns child. createChild = (node,parent)=>new SomeNode(node,parent,...) // etc..</param>
        public static void ConnectNodes(IList<INode> nodes, int countOfConnections, Random rand = null, Func<INode,INode, IChild> createChild = null)
        {
            rand ??= new Random();
            createChild ??= (node,parent) => new Child(node);
            countOfConnections = countOfConnections > nodes.Count ? nodes.Count : countOfConnections;

            foreach (var node in nodes)
            {
                var start_index = rand.Next(nodes.Count);
                ConnectNodeToNodes(node,nodes,start_index,countOfConnections,createChild);
            }
        }
        /// <summary>
        /// Randomly add to each node's Children another nodes, but create random count of connections per node. It connect nodes to each other.
        /// </summary>
        /// <param name="nodes">Nodes to connect</param>
        /// <param name="minCountOfNodes">Min count of children of each node</param>
        /// <param name="maxCountOfNodes">Max count of children of each node</param>
        /// <param name="rand">Use your own rand if you need to get the same output per invoke. Let it null to use new random.</param>
        /// <param name="createChild">Method that consists of node, parent and returns child. createChild = (node,parent)=>new SomeNode(node,parent,...) // etc..</param>
        public static void ConnectRandomCountOfNodes(IList<INode> nodes, int minCountOfNodes, int maxCountOfNodes, Random rand = null, Func<INode,INode, IChild> createChild = null)
        {
            rand ??= new Random();
            createChild ??= (node,parent) => new Child(node);
            minCountOfNodes = minCountOfNodes < 1 ? 1 : minCountOfNodes;
            maxCountOfNodes = maxCountOfNodes > nodes.Count ? nodes.Count : maxCountOfNodes;

            //swap using xor
            if (minCountOfNodes > maxCountOfNodes)
            {
                minCountOfNodes = minCountOfNodes ^ maxCountOfNodes;
                maxCountOfNodes = minCountOfNodes ^ maxCountOfNodes;
                minCountOfNodes = minCountOfNodes ^ maxCountOfNodes;
            }

            foreach (var node in nodes)
            {
                int count_of_connections = rand.Next(maxCountOfNodes-minCountOfNodes)+minCountOfNodes;
                var start_index = rand.Next(nodes.Count);
                ConnectNodeToNodes(node,nodes,start_index,count_of_connections,createChild);
            }
        }
        
        /// <summary>
        /// Removes parent's node from it's children connection.
        /// </summary>
        /// <param name="nodes"></param>
        public static void MakeDirected(IList<INode> nodes){
            foreach(var n in nodes){
                foreach(var child in n.Children){
                    foreach(var c in child.Node.Children){
                        if(c.Node.CompareTo(n)==0){
                            child.Node.Children.Remove(c);
                            break;
                        }
                    }
                }
            }
        }
        public static void MakeUndirected(IList<INode> nodes,Func<INode,INode, IChild> createChild = null){
            createChild ??= (node,parent) => new Child(node);
            
            foreach(var n in nodes){
                foreach(var c in n.Children){
                    if(c.Node.Id==n.Id) continue;
                    if(c.Node.Children.FirstOrDefault(x=>x.Node.Id==n.Id) is null){
                        c.Node.Children.Add(createChild(c.Node,n));
                    }
                }
            }
        }
        //connect some node to List of nodes with some parameters.
        private static void ConnectNodeToNodes(INode node, IList<INode> nodes,int start_index, int count_of_connections,Func<INode,INode, IChild> createChild)
        {
            lock(node)
            for (int i = 0; i < count_of_connections; i++)
            {
                var child = nodes[(start_index + i) % nodes.Count];
                if(child.Id == node.Id) {
                    start_index++;
                    i--;
                    continue;
                }
                node.Children.Add(createChild(child,node));
                
            }
        }

    }
}