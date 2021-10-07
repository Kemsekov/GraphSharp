using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GraphSharp;


const int nodes_count = 30000;
const int min_nodes = 1;
const int max_nodes = 20;
const int steps_count = 10000;

Console.ForegroundColor = ConsoleColor.Green;

var watch1 = new Stopwatch();
watch1.Start();
var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(nodes_count,min_nodes,max_nodes);
System.Console.WriteLine($"Time {watch1.ElapsedMilliseconds} milliseconds to create nodes");
watch1.Stop();
var graph = new Graph(nodes);

var vesitor = new ActionVesitor(node=>{
    Task.Delay(50).Wait();
});

var watch2 = new Stopwatch();
watch2.Start();

graph.Start();

for(int i = 0;i<steps_count;i++){
    graph.Step();
}
System.Console.WriteLine($"Time {watch2.ElapsedMilliseconds} milliseconds to work");
Console.ResetColor();
watch2.Stop();

