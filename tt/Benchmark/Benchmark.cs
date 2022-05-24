using System;
using System.Diagnostics;
using GraphSharp.GraphStructures;

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

var configuration = new EmptyGraphConfiguration();

GraphStructure<EmptyNode,EmptyEdge> graph = default;

var timer = MeasureTime(()=>{
    graph =new GraphStructure<EmptyNode,EmptyEdge>(configuration)
        .Create(nodes_count);
    graph.Do.ConnectNodes(edges_count);
});

Console.ForegroundColor = ConsoleColor.Green;
System.Console.WriteLine($"Time {timer.ElapsedMilliseconds} milliseconds to create nodes");

var visitor = new EmptyVisitor(graph);
visitor.SetGraph(graph);
visitor.SetPosition(0);
timer = MeasureTime(()=>{
    for (int i = 0; i < steps_count; i++)
    {
        visitor.Propagate();
    }
});

Console.WriteLine($"Time {timer.ElapsedMilliseconds} milliseconds to do {steps_count} steps with {nodes_count} nodes and {edges_count} edges");
Console.ResetColor();

