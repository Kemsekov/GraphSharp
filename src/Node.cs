using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GraphSharp
{
    /// <summary>
    /// Supports multiple vesitors
    /// </summary>
    public class Node : NodeBase
    {
        Dictionary<IVesitor,bool> vesited = new();
        public Node(int id) : base(id)
        {
        }
        
        public override void EndVesit(IVesitor vesitor){
            lock(Childs)
                vesited[vesitor] = false;
        }
        public override NodeBase Vesit(IVesitor vesitor)
        {
            lock(Childs){
                if(vesited[vesitor]) return null;
                vesitor.Vesit(this);
                vesited[vesitor] = true;
                return this;
            }
        }
    }
}