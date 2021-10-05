using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
namespace GraphSharp
{
    public static class NodeGraphFactory
    {
        public static List<Node> CreateConnected<Node>(int count_of_nodes,int count_of_childs)
        where Node : NodeBase, new()
        {
            
            var nodes = new List<Node>();
            var rand = new Random();

            for(int i = 0; i<count_of_nodes;i++){
                var node = Activator.CreateInstance(typeof(Node),i) as Node;
                nodes.Add(node);
            }
            foreach (var n in nodes){
                for(int i = 0;i<count_of_childs;i++){
                    Node child;
                    do
                        child = nodes[rand.Next(nodes.Count)];
                    while(child==n);

                    n.AddChild(child);
                }
            }
            return nodes;
        }
        public static List<Node> CreateRandomConnected<Node>(int count_of_nodes,int min_count_of_childs, int max_count_of_childs)
        where Node : NodeBase
        {
            var nodes = new List<Node>();
            foreach(int i in Enumerable.Range(0,count_of_nodes)){
                var node = Activator.CreateInstance(typeof(Node),i) as Node;
                nodes.Add(node);
            }
            ThreadLocal<Random> rand = new ThreadLocal<Random>(()=>new Random());

            Parallel.ForEach(Partitioner.Create(0,nodes.Count),(range,loopState)=>{
                var _rand = rand.Value;

                for(int i = range.Item1;i<range.Item2;i++){
                    var count_of_childs = _rand.Next(max_count_of_childs);
                    
                    if(count_of_childs<min_count_of_childs)
                        count_of_childs = min_count_of_childs;
                    
                    var parent = nodes[i];
                    foreach(int _ in Enumerable.Range(0,count_of_childs)){
                        Node child;
                        do
                            child = nodes[_rand.Next(nodes.Count)];
                        while(child==parent || child.Childs.Contains(parent));
                        parent.AddChild(child);
                    }    
                }
            });
            return nodes;
        }
    }
}
