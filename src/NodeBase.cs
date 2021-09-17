using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphSharp
{
    public abstract class NodeBase
    {
        public List<NodeBase> Childs{get;} = new();
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
        public abstract void EndVesit(IVesitor vesitor);
        /// <summary>
        ///  Vesits current node with vesitor
        /// </summary>
        /// <param name="vesitor"></param>
        /// <returns>If this node is vesited first time then returns current node, else null</returns>
        public abstract NodeBase Vesit(IVesitor vesitor);
        public override bool Equals(object obj)
        {
            if(obj is not NodeBase node)
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
    }
}