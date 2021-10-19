using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphSharp.Vesitos;

namespace GraphSharp.Nodes
{
    public abstract class NodeBase : IComparable<NodeBase>
    {
        public abstract List<NodeBase> Childs{get;}
        public abstract Dictionary<IVesitor,NodeStateBase> NodeStates{get;}
        public int Id { get; }
        public NodeBase(int id)
        {
            Id = id;
        }
        
        public void AddChild(NodeBase child)
        {
            if (!Childs.Contains(child))
                Childs.Add(child);
        }

        public override bool Equals(object obj)
        {
            if(!(obj is NodeBase))
                return false;
            return (obj as NodeBase).Id==Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"Node : {Id}";
        }
        public int CompareTo(NodeBase other)=>Id-other.Id;
    }
}