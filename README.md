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

Next you need to create `Vesitor`.

`Vesitor` is object that vesiting your `graph` when you need to 'walk' through it.

```cs
var vesitor = new ActionVesitor(node=>
{
    Console.Writeline(node);
});
```
And now we need add vesitor to graph

```cs
graph.AddVesitor(vesitor);
```
Now our `vesitor` is bound to some node from `nodes` that we loaded to `graph`.
If you wanna your `vesitor` to be bound to some exact node then you can do this

```cs
graph.AddVesitor(vesitor,index);
```

This will bound `vesitor` to node with index `index`.

Then when we can start.

```cs
graph.Start();
```

After invoking this method you will see this in console 
```
Node : NUMBER
```

In the graph we created every node connected to minimum 5 and at least 20 other nodes.
So when we step through graph next time `vesitor` will print to us next 5<x<20 nodes.

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

If you need to clean your graph then use `Clear` function and then you will have to add vesitors again, but nodes inside of 
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

# Vesitor type

To write your own vesitor inherit it from `IVesitor`

```cs
public class ActionVesitor : IVesitor
    {
        private Action<NodeBase> _vesit;

        public ActionVesitor(Action<NodeBase> vesit)
        {
            this._vesit = vesit;
        }

        public void EndVesit(NodeBase node)
        {
            
        }

        public void Vesit(NodeBase node)
        {   
            _vesit(node);
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

