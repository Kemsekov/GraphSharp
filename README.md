[![nuget](https://img.shields.io/nuget/v/Kemsekov.GraphSharp.svg)](https://www.nuget.org/packages/Kemsekov.GraphSharp/) 
# GraphSharp
GraphSharp is a node based implementation of graph.

# Example of usage

First of all we need to create a bunch of nodes
You can do this using `NodeGraphFactory`
```cs
var nodes = NodeGraphFactory.CreateNodes(100);
```
By default `NodeGraphFactory.CreateNodes` creates `List<INode>` of 1000 nodes type of `Node`, where each of them have unique `Id` from 0 to 100 in this case.

Next we need to connect nodes.
This also can be made my `NodeGraphFactory`

```cs
NodeGraphFactory.ConnectNodes(nodes : nodes,count_of_connections: 10);
```

This method will connect each node of `nodes` to other 10 nodes.

If you wanna to have some random range of connections you can use
`ConnectRandomCountOfNodes` method

```cs
NodeGraphFactory.ConnectRandomCountOfNodes(
    nodes: nodes,
    min_count_of_nodes: 2,
    max_count_of_nodes : 10);
```
This one will connect each node to at least 2 nodes and at most 10 nodes.

Or you could connect nodes to each other my yourself. `INode` have a property 
`IList<IChild> Children`, where you could do whatever you want. Just remeber to add children from the same list of nodes.

Note, that `IChild` is just a wrapper for `INode`, so that way you could define `Children` with some properties like weight or some other value that will say something about this node for node to node connection.

Next we need to create visitor and graph.

We define graph with created nodes, create visitor(s), add visitors to graph and bind them to some starting point nodes, and then call method `Step` to propagate visitor(s) trough graph to precise one generation.

```cs
var visitor = new ActionVisitor(
    //this method called once of every node in graph
    visitor : child => Console.WriteLine(child.Node),
    //this method sort out children we need to visit
    selector: child => true,
    //this method called right after visitor is propagated in graph 
    endVisit: () => {}
);

//create graph from nodes we created
var graph = new Graph(nodes);

//add visitor to graph with starting nodes with id = 1 and 2
graph.AddVisitor(visitor,1,2);

System.Console.WriteLine("---Step 1---");
graph.Step(visitor); //propagate this one visitor
System.Console.WriteLine("---Step 2---");
graph.Step();        //propagate trough all added visitors

```
Possible output:

```
---Step 1---
Node 1
Node 2
---Step 2---
Node 99
Node 0
Node 2
Node 3
Node 4
Node 5
Node 6
Node 7
Node 8
Node 98
Node 1
Node 9
```

As you can see, visitor visited node 1 and node 2 in step 1.
In step 2 the same visitor visited node's 1 and node's 2 children in such a way,
that even if node 1 and node 2 connected to node 3(both of them have this node in `Children`), 
then `visit` method of `ActionVisitor` will visit node 3 only once, meanwhile `select` method
will be called both times from node 1 and node 2.
