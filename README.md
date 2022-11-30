[![nuget](https://img.shields.io/nuget/v/Kemsekov.GraphSharp.svg)](https://www.nuget.org/packages/Kemsekov.GraphSharp/) 
# GraphSharp
GraphSharp is a tool to manipulate on the set of connected nodes, or just graph. 
It allow you to create such algorithms as dijkstra algorithm(shortest path finder), graph coloring, components finder etc...
Also, this library have adapter for graph structure to works as one from [QuikGraph](https://github.com/KeRNeLith/QuikGraph).
So I could call this lib like an extension for QuikGraph library at this point.

For samples see https://github.com/Kemsekov/GraphSharp.Samples

[Dijkstra algorithm](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/Dijkstra's%20algorithm)
![example](https://user-images.githubusercontent.com/57869319/149961444-a0afc184-7119-4a8c-99de-4d15f587559f.jpg)
[Graph coloring (Greedy, DSatur, RLF)](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/GraphColoring)
![example](https://user-images.githubusercontent.com/57869319/161608380-1e82a976-16bc-4fca-a249-c5aa0efdb948.jpg)
[Delaunay triangulation](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/DelaunayTriangulation)
![example](https://user-images.githubusercontent.com/57869319/174455462-f0a7b769-33b8-47b9-b6a6-2936c02f4cbb.jpg)
[Minimal spanning tree](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/Tree)
![example](https://user-images.githubusercontent.com/57869319/174455464-e4b8723b-0158-4a9c-ace1-a9e7dc423913.jpg)
[Topological sort](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/TopologicalSort)
![example](https://user-images.githubusercontent.com/57869319/174638380-b39624b7-8c99-4544-a69b-f99f589d72b4.jpg)
[Find articulation points](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/ArticulationPointsFinder)
![example](https://user-images.githubusercontent.com/57869319/176494620-2cb92342-aa2d-432f-bfc6-ca503017464d.jpg)
[Find components of a graph](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/FindComponents)
![example](https://user-images.githubusercontent.com/57869319/176998046-e1ba18c7-9f11-4b9d-bd2f-54537d5d4a0a.jpg)
[Cycles basis finder](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/CycleFinder) (here I color 10 shortest cycles found)
![example](https://user-images.githubusercontent.com/57869319/179674538-142bf36b-e760-49d8-9ed6-eed3c512e907.jpg)
[Strongly connected components finder](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/StronglyComponentsFinder)
![example](https://user-images.githubusercontent.com/57869319/181353679-86969151-e88e-4600-8db1-8d9e361e96ce.jpg)
[TravelingSalesmanProblem](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/TravelingSalesmanProblem)
![example](https://user-images.githubusercontent.com/57869319/183226714-827188f0-2f34-4a99-b90d-c6937c5dd41f.jpg)
I have a adapter for `IGraph` to work as graph from [QuikGraph](https://github.com/KeRNeLith/QuikGraph).
[Here is an example how this works](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/PageRank) using PageRank implementation from [QuikGraph](https://github.com/KeRNeLith/QuikGraph).
![example](https://user-images.githubusercontent.com/57869319/187511214-3963fa78-ebf5-4d84-8bac-b483ea70f4b1.jpg)
[Max flow algorithm from left bottom to top right (here max capacity is edge length). The brighter - the more flow goes trough](https://github.com/Kemsekov/GraphSharp.Samples/tree/main/samples/MaxFlow)
![example](https://user-images.githubusercontent.com/57869319/204763293-8687acca-30e2-4d23-98e1-3d01e7d192e4.jpg)
