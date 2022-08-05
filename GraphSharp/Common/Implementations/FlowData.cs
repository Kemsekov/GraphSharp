using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common
{
    public struct FlowData
    {
        public FlowData(float flow, float capacity)
        {
            Capacity = capacity;
            Flow = flow;
        }
        public float Capacity{get;set;}
        public float Flow{get;set;}
    }
}