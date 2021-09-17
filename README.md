# GraphSharp
GraphSharp is a node based implementation of graph.

# Example of usage

# Creating nodes
First of all we need to create a bunch of connected nodes
You can do this using `NodeGraphFactory`
```cs
var nodes = NodeGraphFactory.CreateConnected<Node>(1000,20);
```

This will create a 1000 nodes of type `Node` with each of them connected to exact other 20 nodes

If you want more random in count of childs then use

```cs
var nodes = NodeGraphFactory.CreateRandomConnected<Node>(1000,5,20);
```

Which will create 1000 nodes with each of them connected to at least 5 and at max 20 others nodes

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

Because of `NodeGraphFactory` implementation this constraint must be followed on each custom `Node` type

