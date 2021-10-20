using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphSharp.Nodes;

namespace GraphSharp
{
    public static class NodeGraphFactory
    {
        /// <summary>
        /// Create nodes with each of them have approximately <see cref="count_of_childs"/> childs in parallel
        /// </summary>
        /// <param name="count_of_nodes">Count of nodes to create</param>
        /// <param name="count_of_childs">Count of childs for each node</param>
        /// <typeparam name="_Node">Type inherited from <see cref="NodeBase"/></typeparam>
        /// <returns>Created nodes</returns>
        public static List<_Node> CreateConnectedParallel<_Node>(int count_of_nodes, int count_of_childs)
        where _Node : NodeBase
        {

            var nodes = new List<_Node>(count_of_nodes);
            var rand = new Random();

            //create nodes
            for (int i = 0; i < count_of_nodes; i++)
            {
                var node = Activator.CreateInstance(typeof(_Node), i) as _Node;
                nodes.Add(node);
            }

            //create childs
            Parallel.ForEach(nodes, (node, _) =>
             {
                 List<NodeBase> copy = new List<NodeBase>(nodes.GetRange(rand.Next(nodes.Count - count_of_childs), count_of_childs));
                //copy.Shuffle();
                copy.Remove(node);
                 node.Childs.AddRange(copy);
             });

            return nodes;
        }
        /// <summary>
        /// Creates count_of_nodes nodes with >= min_count_of_childs and <= max_count_of_childs childs in parallel.
        /// </summary>
        /// <param name="count_of_nodes">Count of nodes to create</param>
        /// <param name="max_count_of_childs">Max count of childs per node</param>
        /// <param name="min_count_of_childs">Min count of childs per node</param>
        /// <typeparam name="_Node">Type inherited from <see cref="NodeBase"/></typeparam>
        /// <returns>Created nodes</returns>
        public static List<_Node> CreateRandomConnectedParallel<_Node>(int count_of_nodes, int max_count_of_childs, int min_count_of_childs)
        where _Node : NodeBase
        {
            var nodes = new List<_Node>();
            //create nodes
            foreach (int i in Enumerable.Range(0, count_of_nodes))
            {
                var node = Activator.CreateInstance(typeof(_Node), i) as _Node;
                nodes.Add(node);
            }

            //create childs
            ThreadLocal<Random> rand_local = new ThreadLocal<Random>(() => new Random());

            //swap
            if (min_count_of_childs > max_count_of_childs)
            {
                var b = min_count_of_childs;
                max_count_of_childs = min_count_of_childs;
                min_count_of_childs = b;
            }

            Parallel.ForEach(nodes, (node, _) =>
             {
                 var rand = rand_local.Value;
                 var count_of_childs = rand.Next(max_count_of_childs - min_count_of_childs) + min_count_of_childs + 1;
                 List<NodeBase> copy = new List<NodeBase>(nodes.GetRange(rand.Next(nodes.Count - count_of_childs), count_of_childs));
                 copy.Remove(node);
                 node.Childs.AddRange(copy);
             });

            return nodes;
        }
        /// <summary>
        /// Creates count_of_nodes nodes with >= min_count_of_childs and <= max_count_of_childs childs.
        /// </summary>
        /// <param name="count_of_nodes">Count of nodes to create</param>
        /// <param name="max_count_of_childs">Max count of childs per node</param>
        /// <param name="min_count_of_childs">Min count of childs per node</param>
        /// <typeparam name="_Node">Type inherited from <see cref="NodeBase"/></typeparam>
        /// <returns>Created nodes</returns>
        public static List<_Node> CreateRandomConnected<_Node>(int count_of_nodes, int max_count_of_childs, int min_count_of_childs, Random rand = null)
        where _Node : NodeBase
        {
            rand = rand ?? new Random();
            var nodes = new List<_Node>();
            //create nodes
            foreach (int i in Enumerable.Range(0, count_of_nodes))
            {
                var node = Activator.CreateInstance(typeof(_Node), i) as _Node;
                nodes.Add(node);
            }

            if (min_count_of_childs > max_count_of_childs)
            {
                var b = min_count_of_childs;
                max_count_of_childs = min_count_of_childs;
                min_count_of_childs = b;
            }
            //create childs

            foreach (var node in nodes)
            {
                var count_of_childs = rand.Next(max_count_of_childs - min_count_of_childs) + min_count_of_childs + 1;
                List<NodeBase> copy = new List<NodeBase>(nodes.GetRange(rand.Next(nodes.Count - count_of_childs), count_of_childs));
                copy.Remove(node);
                node.Childs.AddRange(copy);
            }

            return nodes;
        }
        /// <summary>
        /// Create nodes with each of them have approximately <see cref="count_of_childs"/> childs.
        /// </summary>
        /// <param name="count_of_nodes">Count of nodes to create</param>
        /// <param name="count_of_childs">Count of childs for each node</param>
        /// <typeparam name="_Node">Type inherited from <see cref="NodeBase"/></typeparam>
        /// <returns>Created nodes</returns>
        public static List<_Node> CreateConnected<_Node>(int count_of_nodes, int count_of_childs, Random rand = null) where _Node : NodeBase
        {
            rand = rand ?? new Random();
            var nodes = new List<_Node>(count_of_nodes);

            //create nodes
            for (int i = 0; i < count_of_nodes; i++)
            {
                var node = Activator.CreateInstance(typeof(_Node), i) as _Node;
                nodes.Add(node);
            }

            //create childs
            foreach (var node in nodes)
            {
                List<NodeBase> copy = new List<NodeBase>(nodes.GetRange(rand.Next(nodes.Count - count_of_childs), count_of_childs));
                copy.Remove(node);
                node.Childs.AddRange(copy);
            };

            return nodes;
        }

    }
}
