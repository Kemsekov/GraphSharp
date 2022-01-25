using System;
using System.Diagnostics;
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Visitors;

Stopwatch MeasureTime(Action operation)
{
    var watch = new Stopwatch();
    watch.Start();
    operation();
    watch.Stop();
    return watch;
}

const int nodes_count = 11000;
const int edges_count = 20;
const int steps_count = 2400;


NodesFactory nodes = default;

var timer = MeasureTime(()=>{
    nodes =
        new NodesFactory()
        .CreateNodes(nodes_count)
        .ForEach()
        .ConnectNodes(edges_count);
});

Console.ForegroundColor = ConsoleColor.Green;
System.Console.WriteLine($"Time {timer.ElapsedMilliseconds} milliseconds to create nodes");

var graph = new Graph(nodes);
var visitor = new ActionVisitor(node => {});
graph.AddVisitor(visitor);

timer = MeasureTime(()=>{
    for (int i = 0; i < steps_count; i++)
    {
        graph.Step();
    }
});

Console.WriteLine($"Time {timer.ElapsedMilliseconds} milliseconds to do {steps_count} steps with {nodes_count} nodes and {edges_count} edges");
Console.ResetColor();

