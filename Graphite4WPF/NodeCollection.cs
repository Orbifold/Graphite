using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Orbifold.Graphite
{
    /// <summary>
    /// Custom collection for nodes.
    /// </summary>
    internal sealed class NodeCollection : Collection<Node>
    {
        private readonly Graph graph;

        internal NodeCollection(Graph parentGraph)
        {
            graph = parentGraph;
        }

        /// <summary>
        /// Gets the children nodes of the specified node.
        /// </summary>
        /// <param name="ofNode">A node in the collection.</param>
        /// <returns></returns>
        public IList<Node> Children(Node ofNode)
        {
            return graph.AdjacentNodes(ofNode);
        }

        /// <summary>
        /// Gets the parent nodes of the specified node.
        /// </summary>
        /// <param name="ofNode">A node in the collection.</param>
        /// <returns></returns>
        public IList<Node> Parents(Node ofNode)
        {
            return graph.AdjacentNodes(ofNode);
        }

        
       
    }
}