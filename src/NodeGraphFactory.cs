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
        public static void ConnectNodes(IList<INode> nodes, int count_of_connections, Random rand = null, Func<INode, IChild> createChild = null)
        {
            rand ??= new Random();
            createChild ??= node => new Child(node);
            count_of_connections = count_of_connections > nodes.Count ? nodes.Count : count_of_connections;

            foreach (var node in nodes)
            {
                var start_index = rand.Next(nodes.Count);
                ConnectNodeToNodes(node,nodes,start_index,count_of_connections,createChild);
            }
        }
        public static void ConnectRandomCountOfNodes(IList<INode> nodes, int min_count_of_nodes, int max_count_of_nodes, Random rand = null, Func<INode, IChild> createChild = null)
        {
            rand ??= new Random();
            createChild ??= node => new Child(node);
            min_count_of_nodes = min_count_of_nodes < 1 ? 1 : min_count_of_nodes;
            max_count_of_nodes = max_count_of_nodes > nodes.Count ? nodes.Count : max_count_of_nodes;

            //swap using xor
            if (min_count_of_nodes > max_count_of_nodes)
            {
                min_count_of_nodes = min_count_of_nodes ^ max_count_of_nodes;
                max_count_of_nodes = min_count_of_nodes ^ max_count_of_nodes;
                min_count_of_nodes = min_count_of_nodes ^ max_count_of_nodes;
            }

            foreach (var node in nodes)
            {
                int count_of_connections = rand.Next(max_count_of_nodes-min_count_of_nodes)+min_count_of_nodes;
                var start_index = rand.Next(nodes.Count);
                ConnectNodeToNodes(node,nodes,start_index,count_of_connections,createChild);
            }
        }
        private static void ConnectNodeToNodes(INode node, IList<INode> nodes,int start_index, int count_of_connections,Func<INode, IChild> createChild)
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
                node.Children.Add(createChild(child));
                
            }
        }
    }
}