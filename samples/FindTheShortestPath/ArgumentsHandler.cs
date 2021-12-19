public class ArgumentsHandler
{
    public int nodeSeed = 0;
    public int connectionSeed = 123;
    public int nodesCount = 200;
    public int node1 = 0;
    public int node2 = 10;
    public int minChildren = 1;
    public int maxChildren = 4;
    public int steps = 100;
    public float thickness = 0.01f;
    public int outputResolution = 1500;
    public float nodeSize = 0.015f;
    public float fontSize = 0.012f;
    public ArgumentsHandler(dynamic paramz)
    {
        nodeSize = paramz.nodeSize;
        nodeSeed = paramz.nodeSeed;
        connectionSeed = paramz.connectionSeed;
        nodesCount = paramz.nodesCount;
        node1 = paramz.node1;
        node2 = paramz.node2;
        minChildren = paramz.minChildren;
        maxChildren = paramz.maxChildren;
        thickness = paramz.thickness;
        steps = paramz.steps;
        outputResolution = paramz.outputResolution;
        fontSize = paramz.fontSize;
    }
    public ArgumentsHandler() => Randomize();
    public void Randomize()
    {
        var rand = new Random();
        nodeSeed = rand.Next();
        connectionSeed = rand.Next();
        nodesCount = rand.Next(300) + 200;
        node1 = rand.Next(nodesCount);
        node2 = (rand.Next(nodesCount) + 1) % nodesCount;
        minChildren = rand.Next(3) + 1;
        maxChildren = rand.Next(6) + 1;
        steps = 100;
        outputResolution = 1500;
    }
}