using System;
using GraphSharp.Common;

namespace GraphSharp
{
    public interface IEdge : IComparable<IEdge>, ICloneable<IEdge>, IFlowed, IWeighted, IColored
    {
        int SourceId{get;set;}
        int TargetId{get;set;}
        int IComparable<IEdge>.CompareTo(IEdge? other){
            if(other is null)
                return 1;
            var d1 = SourceId-other.SourceId;
            var d2 = TargetId-other.TargetId;
            if(d1==0) return d2;
            return d1;
        }
    }
}