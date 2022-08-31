namespace GraphSharp.Common;
public interface IFlowed
{
    /// <summary>
    /// Contains information about flow value, capacity and residual capacity of an edge.
    /// </summary>
    FlowData Flow { get; set; }
}