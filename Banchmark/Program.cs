using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GraphSharp;


const int nodes_count = 20000;
const int min_nodes = 100;
const int max_nodes = 1000;
const int steps_count = 1000;

var watch1 = new Stopwatch();
var nodes = NodeGraphFactory.CreateRandomConnected<Node>(nodes_count,min_nodes,max_nodes);

var graph = new Graph(nodes);

var vesitor = new ActionVesitor(node=>{
    Task.Delay(50).Wait();
});

var watch = new Stopwatch();
watch.Start();

graph.Start();

for(int i = 0;i<steps_count;i++){
    graph.Step();
}

System.Console.WriteLine($"Time {watch.Elapsed.TotalMilliseconds} milliseconds to work");
watch.Stop();

