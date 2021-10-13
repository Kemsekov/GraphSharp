using System.Collections.Generic;
using System.Threading.Tasks;
using GraphSharp.Vesitos;

namespace GraphSharp.Nodes
{
    public abstract class NodeBase
    {
        public List<int> Childs{get;} = new List<int>();
        public int Id { get; }
        public NodeBase(int id)
        {
            Id = id;
        }
        public void AddChild(NodeBase child)
        {
            if (!Childs.Contains(child.Id))
                Childs.Add(child.Id);
        }
        public abstract void EndVesit(IVesitor vesitor);
        public abstract Task EndVesitAsync(IVesitor vesitor);

        /// <summary>
        ///  Vesits current node with vesitor
        /// </summary>
        /// <param name="vesitor"></param>
        /// <returns>If this node is vesited first time then returns current node, else null</returns>
        public abstract NodeBase Vesit(IVesitor vesitor);
        public abstract Task<NodeBase> VesitAsync(IVesitor vesitor);
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
        public static bool operator==(NodeBase t1, NodeBase t2)=>t1.Id==t2.Id;
        public static bool operator!=(NodeBase t1, NodeBase t2)=>t1.Id!=t2.Id;
    }
}