using System;
using GraphSharp.Edges;
using GraphSharp.Nodes;

public class NodeXY : Node
{
    public NodeXY(int id, double x, double y) : base(id)
    {
        X = x;
        Y = y;
    }
    public double X{get;init;}
    public double Y{get;init;}
    public double Distance(NodeXY other){
        return Math.Sqrt((X-other.X)*(X-other.X)+(Y-other.Y)*(Y-other.Y));
    }
    public override string ToString()
    {
        return $"{Id}\t({(float)X}\t{(float)Y})";
    }
}