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
        public static List<_Node> CreateConnectedParallelAsync<_Node>(int count_of_nodes,int count_of_childs)
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
            Parallel.ForEach(nodes,(node,_)=>
            {
                List<NodeBase> copy = new List<NodeBase>(nodes.GetRange(rand.Next(nodes.Count-count_of_childs),count_of_childs));
                //copy.Shuffle();
                copy.Remove(node);
                node.Childs.AddRange(copy.Select(v=>v.Id));
            });

            return nodes;
        }
        public static List<_Node> CreateRandomConnectedParallelAsync<_Node>(int count_of_nodes,int max_count_of_childs,int min_count_of_childs)
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

            //swap
            if(min_count_of_childs>max_count_of_childs){
                var b = min_count_of_childs;
                max_count_of_childs = min_count_of_childs;
                min_count_of_childs = b;
            }
                
            Parallel.ForEach(nodes,(node,_)=>
            {
                var rand = rand_local.Value;
                var count_of_childs = rand.Next(max_count_of_childs-min_count_of_childs)+min_count_of_childs+1;
                List<NodeBase> copy = new List<NodeBase>(nodes.GetRange(rand.Next(nodes.Count-count_of_childs),count_of_childs));
                copy.Remove(node);
                node.Childs.AddRange(copy.Select(v=>v.Id));
            });

            return nodes;
        }
        public static List<_Node> CreateConnectedParallel<_Node>(int count_of_nodes,int count_of_childs) where _Node : NodeBase{
            return CreateConnectedParallelAsync<_Node>(count_of_nodes,count_of_childs);
        }
        public static List<_Node> CreateRandomConnectedParallel<_Node>(int count_of_nodes,int max_count_of_childs,int min_count_of_childs) 
        where _Node : NodeBase
        {
            return CreateRandomConnectedParallelAsync<_Node>(count_of_nodes,max_count_of_childs,min_count_of_childs);
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

            if(min_count_of_childs>max_count_of_childs){
                var b = min_count_of_childs;
                max_count_of_childs = min_count_of_childs;
                min_count_of_childs = b;
            }
            //create childs

            foreach(var node in nodes)
            {
                var count_of_childs = rand.Next(max_count_of_childs-min_count_of_childs)+min_count_of_childs+1;
                List<NodeBase> copy = new List<NodeBase>(nodes.GetRange(rand.Next(nodes.Count-count_of_childs),count_of_childs));
                copy.Remove(node);
                node.Childs.AddRange(copy.Select(v=>v.Id));
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
                List<NodeBase> copy = new List<NodeBase>(nodes.GetRange(rand.Next(nodes.Count-count_of_childs),count_of_childs));
                copy.Remove(node);
                node.Childs.AddRange(copy.Select(v=>v.Id));
            };

            return nodes;
        }
    }
}
