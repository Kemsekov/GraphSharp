using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Vesitos;
using System.Threading.Tasks.Dataflow;
using System.Threading;

using var log = File.OpenWrite("log.txt");
SemaphoreSlim semaphore = new SemaphoreSlim(1);

async Task Log(string message){
    await semaphore.WaitAsync();
    log.Write(Encoding.UTF8.GetBytes(message));
    semaphore.Release();
}


async Task LogLn(string msg){
    await Log(msg+'\n');
}

const int nodes_count = 20000;
const int min_nodes = 1;
const int max_nodes = 20;
const int steps_count = 20;

Console.ForegroundColor = ConsoleColor.Green;

var watch1 = new Stopwatch();
watch1.Start();
var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(nodes_count,min_nodes,max_nodes);
System.Console.WriteLine($"Time {watch1.ElapsedMilliseconds} milliseconds to create nodes");
watch1.Stop();
var graph = new Graph(nodes);
var vesitor = new ActionVesitor(async node=>{
    await Log($"Node {node.Id} ");
});

var watch2 = new Stopwatch();
watch2.Start();
graph.AddVesitor(vesitor);

graph.Start();

LogLn("\n---Start---").Wait();
for(int i = 0;i<steps_count;i++){
    graph.Step();
    LogLn($"\n---Step {i}---").Wait();
}
System.Console.WriteLine($"Time {watch2.ElapsedMilliseconds} milliseconds to work");
Console.ResetColor();
watch2.Stop();

