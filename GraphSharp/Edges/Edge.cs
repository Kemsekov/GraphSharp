using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Common;


namespace GraphSharp
{
    /// <summary>
    /// Default edge
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public class Edge : IEdge
    {
        public int SourceId{get;set;}
        public int TargetId{get;set;}
        public FlowData Flow {get;set;}
        public float Weight {get;set;}
        public Color Color {get;set;}

        public Edge(INode source, INode target)
        {
            SourceId = source.Id;
            TargetId = target.Id;
        }
        public Edge(int sourceId, int targetId)
        {
            SourceId = sourceId;
            TargetId = targetId;
        }
        public override string ToString()
        {
            return $"Edge {SourceId}->{TargetId}";
        }

        public virtual IEdge Clone()
        {
            return new Edge(SourceId,TargetId){
                Weight = this.Weight, 
                Color=this.Color,
                Flow=this.Flow
            };
        }
    }
}