using GraphSharp;
using GraphSharp.Visitors;
using GraphSharp.Children;
using GraphSharp.Nodes;
using GraphSharp.Graphs;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

//this program showing how to find the shortest path betwen two nodes
//by summing and comparing sum of visited path

var rand = new Random(0);
var nodes = NodeGraphFactory.CreateNodes(400,id=>new NodeXY(id,rand.NextDouble(),rand.NextDouble()));

// NodeGraphFactory.ConnectRandomCountOfNodes(nodes, 0, 2, rand, (node, parent) => new NodeConnector(node, parent));

foreach(var m in nodes){ 
    if(m is NodeXY n)
    for(int i = rand.Next(5)+1;i>0;--i){
        (NodeXY node,double distance) min = (null,0);
        foreach(var child in nodes){
            if(child is NodeXY pretendent){
                if(pretendent.Id==n.Id) continue;
                if(min.node is null){
                    min = (pretendent,n.Distance(pretendent));
                    continue;
                }
                var pretendent_distance = n.Distance(pretendent);
                if(pretendent_distance<min.distance && n.Children.FirstOrDefault(x=>x.Node.Id==pretendent.Id) is null){
                    min = (pretendent,pretendent_distance);                        
                }
            }
        }
        min.node.Children.Add(new NodeConnector(n,min.node));
        n.Children.Add(new NodeConnector(min.node,n));
    }

}


var startNode = nodes[310];
var endNode = nodes[55];

var pathFinder = new PathFinder(startNode);

//~1500 ms
// var graph = new Graph(nodes);
//~500 ms
var graph = new ParallelGraph(nodes);
graph.AddVisitor(pathFinder, startNode.Id);

System.Console.WriteLine($"Trying to find path from {startNode} to {endNode}...");
Helpers.MeasureTime(() =>{
    for (int i = 0; i < 30; i++)
        graph.Step();
});

var path = pathFinder.GetPath(endNode) ?? new List<INode>();
Helpers.ValidatePath(path);

Helpers.PrintPath(path);
using var image = new Image<Rgba32>(1500,1500);

var drawer = new GraphDrawer(Brushes.Solid(Color.Brown),0.015f,Brushes.Solid(Color.BlueViolet),0.004f,0.012f*(image.Width+image.Height)/2);
drawer.Clear(image,Color.Black);
drawer.DrawNodeConnections(image,nodes);
drawer.DrawNodes(image,nodes);

if(path.Count>0){
    drawer.DrawLineBrush = Brushes.Solid(Color.Wheat);
    drawer.DrawPath(image,path);
}
image.SaveAsJpeg("file.jpg");

System.Console.WriteLine($"---Path length {pathFinder.GetPathLength(endNode)}");
