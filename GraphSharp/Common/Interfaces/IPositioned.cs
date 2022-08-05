using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace GraphSharp.Common
{
    public interface IPositioned
    {
        Vector2 Position{get;set;}
    }
}