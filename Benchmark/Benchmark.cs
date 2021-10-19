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

const int nodes_count = 9000;
const int min_nodes = 2;
const int max_nodes = 20;
const int steps_count = 1200*2;

Console.ForegroundColor = ConsoleColor.Green;

var watch1 = new Stopwatch();
watch1.Start();
var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(nodes_count,min_nodes,max_nodes);
System.Console.WriteLine($"Time {watch1.ElapsedMilliseconds} milliseconds to create nodes");
watch1.Stop();
var graph = new Graph(nodes);
var visitor = new ActionVisitor((node,visited)=>{
    
});

var watch2 = new Stopwatch();
watch2.Start();
graph.AddVisitor(visitor);

for(int i = 0;i<steps_count;i++){
    graph.Step();
}
System.Console.WriteLine($"Time {watch2.ElapsedMilliseconds} milliseconds to work");
// System.Console.WriteLine($"Step time {graph._StepTroughGen}");
// System.Console.WriteLine($"End visit time {graph._EndVisit}");

Console.ResetColor();
watch2.Stop();