using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphSharp.Common
{
    public interface IFlowed
    {
        /// <summary>
        /// Amount of flow currently going trough
        /// </summary>
        float Flow{get;set;}
        /// <summary>
        /// Maximum amount of flow that must not exceed
        /// </summary>
        float Capacity{get;set;}
    }
}