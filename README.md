# GraphSharp
GraphSharp is a node based implementation of graph.

# Example of usage

First of all we need to create a bunch of connected nodes
You can do this using `NodeGraphFactory`
```cs
var nodes = NodeGraphFactory.CreateConnected<Node>(1000,20);
```

This will create a 1000 nodes of type `Node` with each of them connected to approximately other 20 nodes

If you want more random in count of childs then use

```cs
var nodes = NodeGraphFactory.CreateRandomConnected<Node>(1000,5,20);
```

Which will create 1000 nodes with each of them connected to at least 5 and at max 20 others nodes

Next what you need to create `Graph` and pass it nodes

```cs
var graph = new Graph(nodes);
```

Next you need to create `Visitor`.

`Visitor` is object that visiting your `graph` when you need to 'walk' through it.

```cs
var visitor = new ActionVisitor((node,visited)=>
{
    //if we printed it earlier do not print it again
    if(!visited)
    Console.Writeline(node);
});
```
In this one we wanna write all nodes that we visit to console except those which we already visited.

And now we need add visitor to graph

```cs
graph.AddVisitor(visitor);
```
Now our `visitor` is bound to some node from `nodes` that we loaded to `graph`.
If you wanna your `visitor` to be bound to some exact nodes then you can do this

```cs
graph.AddVisitor(visitor,index1,index2...);
```

This will bound `visitor` to nodes with `Id` equals to corresponding `indexN`.

Then when we can start.

```cs
graph.Step();
```

After invoking this method you will see this in console 
```
Node : NUMBER
```

In the graph we created every node connected to minimum 5 and at least 20 other nodes.
So when we step through graph next time `visitor` will print to us `Node` childs.

```cs
graph.Step();
```

And in console

```
Node : NUMBER1
Node : NUMBER2
Node : NUMBER3
Node : NUMBER4
...
```

If you need to clean your graph then use `Clear` function and then you will have to add visitors again, but nodes inside of `Graph`
will stay the same.
```cs
graph.Clear();
```

A whole example
```cs
using GraphSharp;
using GraphSharp.Graphs;
using GraphSharp.Nodes;
using GraphSharp.Visitors;

//create 20 nodes with each of them have from 0 to 2 childs
var nodes = NodeGraphFactory.CreateRandomConnectedParallel<Node>(20, 0, 2);

var graph = new Graph(nodes);

//print node if it is visited first time
var visitor = new ActionVisitor((node,visited)=>{
    if(!visited)
        System.Console.WriteLine(node);
});

//add visitor to node 1 and node 2
graph.AddVisitor(visitor,1,2);

System.Console.WriteLine("---Step 1---");
graph.Step();
System.Console.WriteLine("---Step 2---");
graph.Step();
```
Output
```
---Step 1---
Node : 1
Node : 2
---Step 2---
Node : 6
Node : 7
Node : 8
Node : 14
Node : 15
Node : 16
```

# Node type
Your custom `Node` type must be inherited from `NodeBase`.
If you wanna to use your custom `Node` type win `NodeGraphFactory` then it MUST have constructor with single integer as input.
```cs
    public class Node : NodeBase
    {
        public Node(int id) : base(id)
        {
        }
    }
```

# Visitor type

To write your own visitor inherit it from `IVisitor`

```cs
public class Visitor : IVisitor
    {

        public MyVisitor()
        {
        }
        
        //When you call Graph.Step() this method is called first.
        //It will filter which node must be visited, and which not.
        public bool Select(NodeBase node){
            //select only even nodes
            return node.Id % 2 == 0;
        }
        //This method will recieve nodes. visited means whatever current visitor already visited node or not.
        public void Visit(NodeBase node, bool visited)
        {   
            if(!visited)
                Console.Writeline(node);
        }
        //this method ends visiting nodes. Unlike Visit(), which is called on every node and you free
        //to do anything with it(depend of visited value) this method called on node only once.
        public void EndVisit(NodeBase node)
        {
            //do nothing
        }
    }
```

And with this visitor output may be like this after calling `Graph.Step()`.
```
Node : 2
Node : 4
Node : 6
```


# Graph type

To write your own `Graph` type inherit it from `IGraph`

```cs
class Graph : IGraph{
    //...
}
```

# What it is best suited for

Graph is best suited for a not big number of visitors and a lot of nodes.
A lot of visitors and nodes can greatly affect memory usage, so be ware.
