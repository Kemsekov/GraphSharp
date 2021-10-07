using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using GraphSharp.Extensions;

namespace GraphSharp
{
    public static class NodeGraphFactory
    {
        public static async Task<List<_Node>> CreateConnectedParallelAsync<_Node>(int count_of_nodes,int count_of_childs)
        where _Node : NodeBase
        {
            
            var nodes = new List<_Node>(count_of_nodes);
            var rand = new Random();

            //create nodes
            for(int i = 0; i<count_of_nodes;i++){
                var node = Activator.CreateInstance(typeof(_Node),i) as _Node;
                nodes.Add(node);
            }
            
            //create childs
            await nodes.ParallelForEachAsync(async node=>
            {
                List<NodeBase> copy = new List<NodeBase>(nodes);
                copy.Shuffle(count_of_childs);
                copy.Remove(node);
                node.Childs.AddRange(copy.GetRange(0,count_of_childs));
            });

            return nodes;
        }
        public static async Task<List<_Node>> CreateRandomConnectedParallelAsync<_Node>(int count_of_nodes,int max_count_of_childs,int min_count_of_childs)
        where _Node : NodeBase
        {
            var nodes = new List<_Node>();
            //create nodes
            foreach(int i in Enumerable.Range(0,count_of_nodes)){
                var node = Activator.CreateInstance(typeof(_Node),i) as _Node;
                nodes.Add(node);
            }

            //create childs
            ThreadLocal<Random> rand_local = new ThreadLocal<Random>(()=>new Random());

            await nodes.ParallelForEachAsync(async node=>
            {
                var rand = rand_local.Value;
                var count_of_childs = rand.Next(Math.Abs(max_count_of_childs-min_count_of_childs))+min_count_of_childs;
                List<NodeBase> copy = new List<NodeBase>(nodes);
                copy.Shuffle(count_of_childs);
                copy.Remove(node);
                node.Childs.AddRange(copy.GetRange(0,count_of_childs));
            });;

            return nodes;
        }
        public static List<_Node> CreateConnectedParallel<_Node>(int count_of_nodes,int count_of_childs) where _Node : NodeBase{
            return CreateConnectedParallelAsync<_Node>(count_of_nodes,count_of_childs).Result;
        }
        public static List<_Node> CreateRandomConnectedParallel<_Node>(int count_of_nodes,int max_count_of_childs,int min_count_of_childs) 
        where _Node : NodeBase
        {
            return CreateRandomConnectedParallelAsync<_Node>(count_of_nodes,max_count_of_childs,min_count_of_childs).Result;
        }
        public static List<_Node> CreateRandomConnected<_Node>(int count_of_nodes,int max_count_of_childs,int min_count_of_childs,Random rand = null) 
        where _Node : NodeBase
        {
            rand = rand ?? new Random();
            var nodes = new List<_Node>();
            //create nodes
            foreach(int i in Enumerable.Range(0,count_of_nodes)){
                var node = Activator.CreateInstance(typeof(_Node),i) as _Node;
                nodes.Add(node);
            }

            //create childs

            foreach(var node in nodes)
            {
                var count_of_childs = rand.Next(Math.Abs(max_count_of_childs-min_count_of_childs))+min_count_of_childs;
                List<NodeBase> copy = new List<NodeBase>(nodes);
                copy.Shuffle(count_of_childs);
                copy.Remove(node);
                node.Childs.AddRange(copy.GetRange(0,count_of_childs));
            }

            return nodes;
        }
        public static List<_Node> CreateConnected<_Node>(int count_of_nodes,int count_of_childs,Random rand = null) where _Node : NodeBase
        {
            rand = rand ?? new Random();
            var nodes = new List<_Node>(count_of_nodes);

            //create nodes
            for(int i = 0; i<count_of_nodes;i++){
                var node = Activator.CreateInstance(typeof(_Node),i) as _Node;
                nodes.Add(node);
            }
            
            //create childs
            foreach(var node in nodes)
            {
                List<NodeBase> copy = new List<NodeBase>(nodes);
                copy.Shuffle(count_of_childs);
                copy.Remove(node);
                node.Childs.AddRange(copy.GetRange(0,count_of_childs));
            };

            return nodes;
        }
    }
}
