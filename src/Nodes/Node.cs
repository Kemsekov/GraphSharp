using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using GraphSharp.Vesitos;

namespace GraphSharp.Nodes
{
    /// <summary>
    /// Supports multiple vesitors
    /// </summary>
    public class Node : NodeBase
    {
        Dictionary<IVesitor, bool> vesited = new Dictionary<IVesitor, bool>();
        SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public Node(int id) : base(id)
        {
        }

        public override void EndVesit(IVesitor vesitor)
        {
            semaphore.Wait();
            vesited[vesitor] = false;
            vesitor.EndVesit(this);
            semaphore.Release();
        }

        public override async Task EndVesitAsync(IVesitor vesitor)
        {
            await semaphore.WaitAsync();
            vesited[vesitor] = false;
            vesitor.EndVesit(this);
            semaphore.Release();
        }

        public override NodeBase Vesit(IVesitor vesitor)
        {
            semaphore.Wait();
            if (vesited[vesitor])
            {
                semaphore.Release();
                return null;
            }
            vesitor.Vesit(this);
            vesited[vesitor] = true;
            semaphore.Release();
            return this;
        }

        public override async Task<NodeBase> VesitAsync(IVesitor vesitor)
        {
            await semaphore.WaitAsync();
            if (vesited[vesitor])
            {
                semaphore.Release();
                return null;
            }
            vesitor.Vesit(this);
            vesited[vesitor] = true;
            semaphore.Release();
            return this;
        }
    }
}