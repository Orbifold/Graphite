using System.Collections;
using System.Collections.Generic;

namespace Orbifold.Graphite
{
    /// <summary>
    /// Custom collection implementation for edges. Technically an <see
    /// cref="http://en.wikipedia.org/wiki/Incidence_matrix">incidence matrix</see>
    /// structure.
    /// </summary>
    /// <remarks>
    /// If you take the adjmatrix[n] you get the collection of parent of node n
    /// </remarks>
    internal sealed class EdgeCollection : IEnumerable<Edge>
    {
        #region Fields
        private Graph graph;
        private readonly Dictionary<Node, List<Node>> adjmatrix;
        #endregion

        #region Constructor
        internal EdgeCollection(Graph parentGraph)
        {
            graph = parentGraph;
            adjmatrix = new Dictionary<Node, List<Node>>();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Adds an edge to the collection.
        /// </summary>
        /// <param name="from">The From or parent node.</param>
        /// <param name="to">The To or child node.</param>
        public void AddEdge(Node from, Node to)
        {
            if (!adjmatrix.ContainsKey(to))
                adjmatrix.Add(to, new List<Node>());
            if (!adjmatrix[to].Contains(from))
                adjmatrix[to].Add(from);
        }
        /// <summary>
        /// Removes an edge from the collection.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public void RemoveEdge(Node from, Node to)
        {
            if (!adjmatrix.ContainsKey(from))
                return;
            if (!adjmatrix[from].Contains(to))
                return;
            adjmatrix[from].Remove(to);
        }
        /// <summary>
        /// Removes the node from the matrix.
        /// </summary>
        /// <param name="node">The node.</param>
        public void RemoveNode(Node node)
        {
            if (adjmatrix.ContainsKey(node))
                adjmatrix.Remove(node); //this removes all the parent bindings
            foreach (Node key in adjmatrix.Keys)
            {
                if (adjmatrix[key].Contains(node))
                    adjmatrix[key].Remove(node);//this removes a children bindings
            }
        }

        /// <summary>
        /// Adds an edge.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="exists">if set to <c>true</c> [exists].</param>
        private void AddEdge(Node from, Node to, bool exists)
        {
            if (exists)
                AddEdge(from, to);
            else
                RemoveEdge(from, to);
        }

        /// <summary>
        /// Alternative access to the <see cref="AddEdge"/> and <see cref="RemoveEdge"/> methods.
        /// </summary>
        /// <value></value>
        public bool this[Node from, Node to]
        {
            set { AddEdge(from, to, value); }
            get
            {
                if (adjmatrix.ContainsKey(from) && adjmatrix[from].Contains(to))
                    return true;
                return false;
            }
        }
        /// <summary>
        /// Gets childrens of the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public List<Node> ChildrenOf(Node node)
        {
            var nodes = new List<Node>();
            foreach (Node key in adjmatrix.Keys)
            {
                if (adjmatrix[key].Contains(node))
                    nodes.Add(key);
            }
            return nodes;
        }
        /// <summary>
        /// Gets the parents of the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public List<Node> ParentsOf(Node node)
        {
            if (adjmatrix.ContainsKey(node))
            {
                return adjmatrix[node];
            }
            return new List<Node>();
        }


        /// <summary>
        /// Returns whether there is an edge between the specified nodes, independently of the direction.
        /// </summary>
        /// <param name="a">From.</param>
        /// <param name="b">To.</param>
        /// <returns>
        /// 	<c>true</c> if the specified a has edge; otherwise, <c>false</c>.
        /// </returns>
        internal bool HasEdge(Node a, Node b)
        {
            if (adjmatrix.ContainsKey(a))
            {
                if (adjmatrix[a].Contains(b))
                {
                    return true;
                }

            }
            if (adjmatrix.ContainsKey(b))
            {
                if (adjmatrix[b].Contains(a))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        internal void Clear()
        {
            adjmatrix.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Edge> GetEnumerator()
        {
            foreach (Node key in adjmatrix.Keys)
            {
                foreach (Node node in adjmatrix[key])
                {
                    yield return new Edge { From = key,To = node};
                }
            }
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}