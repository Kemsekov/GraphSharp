using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Visitors;
using System.Threading.Tasks.Dataflow;
using System.Threading;

const int nodes_count = 11000;
const int edges_count = 20;
const int steps_count = 2400;

Console.ForegroundColor = ConsoleColor.Green;

var watch1 = new Stopwatch();
watch1.Start();
var nodes = 
    new NodesFactory()
    .CreateNodes(nodes_count)
    .ForEach()
    .ConnectNodes(edges_count);

System.Console.WriteLine($"Time {watch1.ElapsedMilliseconds} milliseconds to create nodes");
watch1.Stop();
var graph = new Graph(nodes);
var visitor = new ActionVisitor(node=>{
    
});

var watch2 = new Stopwatch();
watch2.Start();
graph.AddVisitor(visitor);
for(int i = 0;i<steps_count;i++){
    graph.Step();
}

System.Console.WriteLine($"Time {watch2.ElapsedMilliseconds} milliseconds to do {steps_count} steps with {nodes_count} nodes and {edges_count} edges");

Console.ResetColor();
watch2.Stop();

