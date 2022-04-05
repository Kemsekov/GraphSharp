using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Models
{

    public struct WeightedEdge
    {
        public int ParentId;
        public int ChildId;
        public float Weight;

        public WeightedEdge(int parentId, int childId, float weight)
        {
            ParentId = parentId;
            ChildId = childId;
            Weight = weight;
        }
    }
}