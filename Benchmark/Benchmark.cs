using System;
using System.Diagnostics;
using GraphSharp;
using GraphSharp.Propagators;
using GraphSharp.Visitors;
using GraphSharp.GraphStructures;
using GraphSharp.Nodes;
using GraphSharp.Edges;
using GraphSharp.GraphStructures.Implementations;

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

var configuration = new GraphConfiguration<EmptyNode,EmptyEdge>(
    createNode: id => new EmptyNode(id),
    createEdge: (p,c)=> new EmptyEdge(p,c),
    distance: (n1,n2)=>0,
    getNodeWeight: n=>0,
    setNodeWeight: (n,v)=>{},
    getEdgeWeight: e=>0,
    setEdgeWeight: (e,v)=>{}
);

IGraphStructure<EmptyNode> nodes = default;

var timer = MeasureTime(()=>{
    nodes =
        new GraphStructure<EmptyNode,EmptyEdge>(configuration)
        .CreateNodes(nodes_count)
        .ForEach()
        .ConnectNodes(edges_count);
});

Console.ForegroundColor = ConsoleColor.Green;
System.Console.WriteLine($"Time {timer.ElapsedMilliseconds} milliseconds to create nodes");

var visitor = new EmptyVisitor();
visitor.SetNodes(nodes);
visitor.SetPosition(0);
timer = MeasureTime(()=>{
    for (int i = 0; i < steps_count; i++)
    {
        visitor.Propagate();
    }
});

Console.WriteLine($"Time {timer.ElapsedMilliseconds} milliseconds to do {steps_count} steps with {nodes_count} nodes and {edges_count} edges");
Console.ResetColor();

