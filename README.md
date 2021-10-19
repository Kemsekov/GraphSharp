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
var visitor = new ActionVisitor(node=>
{
    Console.Writeline(node);
});
```
And now we need add visitor to graph

```cs
graph.AddVisitor(visitor);
```
Now our `visitor` is bound to some node from `nodes` that we loaded to `graph`.
If you wanna your `visitor` to be bound to some exact node then you can do this

```cs
graph.AddVisitor(visitor,index);
```

This will bound `visitor` to node with index `index`.

Then when we can start.

```cs
graph.Start();
```

After invoking this method you will see this in console 
```
Node : NUMBER
```

In the graph we created every node connected to minimum 5 and at least 20 other nodes.
So when we step through graph next time `visitor` will print to us next 5<x<20 nodes.

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

If you need to clean your graph then use `Clear` function and then you will have to add visitors again, but nodes inside of 
will stay the same.
```cs
graph.Clear();
```

# Node type
`Node` type must be inherited from `NodeBase` class and must have constructor with single integer as input.
```cs
    public class Node : NodeBase
    {
        public Node(int id) : base(id)
        {
        }
    }
```

Because of `NodeGraphFactory` implementation this constraint must be followed on each custom `Node` type.

# Visitor type

To write your own visitor inherit it from `IVisitor`

```cs
public class ActionVisitor : IVisitor
    {
        private Action<NodeBase> _visit;

        public ActionVisitor(Action<NodeBase> visit)
        {
            this._visit = visit;
        }

        public void EndVisit(NodeBase node)
        {
            
        }

        public void Visit(NodeBase node)
        {   
            _visit(node);
        }
    }
```

# Graph type

I am not recommend you to write your own `IGraph` implementation but to do so just inherit it from `IGraph`

```cs
class Graph : IGraph{
    //...
}
```

# What it is best suited for

Graph is best suited for a not big number of visitors and a lot of nodes.
A lot of visitors and nodes can greatly affect memory usage, so be ware.
