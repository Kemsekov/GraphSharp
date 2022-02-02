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
        nodeSize = paramz.nodeSize ?? 0.003;
        nodeSeed = paramz.nodeSeed ?? 0;
        connectionSeed = paramz.connectionSeed ?? 1;
        nodesCount = paramz.nodesCount ?? 1000;
        node1 = paramz.node1 ?? 0;
        node2 = paramz.node2 ?? 100;
        minEdges = paramz.minEdges ?? 0;
        maxEdges = paramz.maxEdges ?? 5;
        thickness = paramz.thickness ?? 0.0015;
        steps = paramz.steps ?? 3000;
        outputResolution = paramz.outputResolution ?? 4000;
        fontSize = paramz.fontSize ?? 0.001;
    }
}