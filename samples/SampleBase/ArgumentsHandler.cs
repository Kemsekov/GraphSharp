using Newtonsoft.Json;

public class ArgumentsHandler
{
    public int nodeSeed = 0;
    public int connectionSeed = 123;
    public int nodesCount = 200;
    public int node1 = 0;
    public int node2 = 10;
    public int minEdges = 1;
    public int maxEdges = 4;
    public int steps = 100;
    public float thickness = 0.01f;
    public int outputResolution = 1500;
    public float nodeSize = 0.015f;
    public float fontSize = 0.012f;
    public ArgumentsHandler(string settingsFile)
    {
        dynamic paramz = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(settingsFile)) ?? new object();
        nodeSize = paramz.nodeSize;
        nodeSeed = paramz.nodeSeed;
        connectionSeed = paramz.connectionSeed;
        nodesCount = paramz.nodesCount;
        node1 = paramz.node1;
        node2 = paramz.node2;
        minEdges = paramz.minEdges;
        maxEdges = paramz.maxEdges;
        thickness = paramz.thickness;
        steps = paramz.steps;
        outputResolution = paramz.outputResolution;
        fontSize = paramz.fontSize;
    }
}