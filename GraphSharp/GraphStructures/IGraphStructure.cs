using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphSharp.Edges;
using GraphSharp.Nodes;

namespace GraphSharp.GraphStructures
{
    public interface IGraphStructure
    {
        /// <summary>
        /// group of nodes that selected to be modified in next invocations.<br/>
        /// For example <see cref="GraphStructure.ForEach"/> will set this property just to <see cref="GraphStructure.Nodes"/> and
        /// next invocations of any operation will be performed on all nodes.
        /// <see cref="GraphStructure.ForOne"/> will set this property to just one particular node from <see cref="GraphStructure.Nodes"/>.
        /// <see cref="GraphStructure.ForNodes"/> will set this property to any subset of <see cref="GraphStructure.Nodes"/> 
        /// </summary>
        /// <value></value>
        IEnumerable<INode> WorkingGroup { get; }
        IList<INode> Nodes { get; }
        /// <summary>
        /// <see cref="Random"/> that used by any <see cref="IGraphStructure"/> to implement's it's logic when it need random values
        /// </summary>
        /// <value></value>
        Random Rand { get; }
        /// <summary>
        /// Method that used to create instance of <see cref="INode"/> from it's <see cref="INode.Id"/> as argument
        /// </summary>
        /// <value></value>
        Func<int, INode> CreateNode { get;}
        /// <summary>
        /// Method that used to create new <see cref="IEdge"/> from two <see cref="INode"/>, where first is node itself and second is it's parent
        /// (node,parent)=>new Edge...
        /// </summary>
        Func<INode, INode, IEdge> CreateEdge { get; }
        /// <summary>
        /// Method that used to get weight from particular <see cref="IEdge"/>
        /// </summary>
        /// <value></value>
        public Func<IEdge,float> GetWeight { get;}
        /// <summary>
        /// Method that used to determite how to calculate distance between two <see cref="INode"/>
        /// </summary>
        /// <value></value>
        public Func<INode,INode,float> Distance{get;}

    }
}