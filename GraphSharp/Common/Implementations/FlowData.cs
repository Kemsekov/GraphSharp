namespace GraphSharp.Common;

/// <summary>
/// Contains <paramref name="Flow"/>, <paramref name="Capacity"/> and <paramref name="ResidualCapacity"/>
/// that needed for flow algorithms
/// </summary>
public struct FlowData
{
    public FlowData(double flow, double capacity)
    {
        Capacity = capacity;
        Flow = flow;
    }
    public double Capacity { get; set; }
    public double Flow { get; set; }
    public double ResidualCapacity => Capacity-Flow;
}