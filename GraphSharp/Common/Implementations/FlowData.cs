namespace GraphSharp.Common;

/// <summary>
/// Contains <paramref name="Flow"/>, <paramref name="Capacity"/> and <paramref name="ResidualCapacity"/>
/// that needed for flow algorithms
/// </summary>
public struct FlowData
{
    public FlowData(float flow, float capacity)
    {
        Capacity = capacity;
        Flow = flow;
    }
    public float Capacity { get; set; }
    public float Flow { get; set; }
    public float ResidualCapacity => Capacity-Flow;
}