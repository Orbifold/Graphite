using System;
using System.Collections.Generic;
using System.Linq;
namespace Orbifold.Graphite
{

    /// <summary>
    /// The model or graph.
    /// </summary>
    internal sealed class Graph
    {

        #region Events

        public event EventHandler<NodeEventArgs> NodeAdded;
        public event EventHandler<NodeEventArgs> NodeRemoved;
        public event EventHandler<EdgeEventArgs> EdgeAdded;
        public event EventHandler<EdgeEventArgs> EdgeRemoved;
        #endregion

        #region Fields

        private readonly EdgeCollection edges;
        private readonly NodeCollection nodes;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a random node from the graph.
        /// </summary>
        /// <value>The random node.</value>
        public Node RandomNode
        {
            get
            {
                var rnd = new Random();
                return nodes[rnd.Next(0, nodes.Count)];
            }
        }
        /// <summary>
        /// Gets the nodes in this graph.
        /// </summary>
        /// <value>The nodes.</value>
        internal NodeCollection Nodes
        {
            get { return nodes; }
        }
        internal EdgeCollection Edges
        {
            get { return edges; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/> class.
        /// </summary>
        public Graph()
        {
            edges = new EdgeCollection(this);
            nodes = new NodeCollection(this);
        }
        #endregion

        #region Methods
        public void Clear()
        {
            edges.Clear();
            nodes.Clear();
        }

        public bool HasEdge(Node from, Node to)
        {
            return edges.HasEdge(from, to);
        }

        public IList<Node> AdjacentNodes(Node a)
        {
            return ChildrenOf(a).Union(ParentsOf(a)).ToList();
        }

        public void AddEdge(Node from, Node to)
        {
            AddEdge(from, to,"");
        }

        public void AddEdge(Node from, Node to, string label)
        {
            if (!nodes.Contains(from) || !nodes.Contains(to))
                throw new Exception("One or both of the nodes attached to the edge is not contained in the graph.");
            edges.AddEdge(from, to);
            RaiseEdgeAdded(from, to,label);

        }
        public void RemoveEdge(Node from, Node to)
        {
            edges.RemoveEdge(from, to);
            RaiseEdgeRemoved(from, to);
        }

        public void AddNode(Node node)
        {
            //HACK: demo restriction
#if DEMO
            if(nodes.Count()>=10)
                throw new ApplicationException("The demo doesn't allow you to add more than ten nodes to the diagram. Please acquire the commercial version for full access.");

#endif
            if (!nodes.Contains(node))
            {
                nodes.Add(node);
                node.Graph = this; //keep an internal binding with the graph alltogether
                RaiseNodeAdded(node);
            }
        }

        public void RemoveNode(Node node)
        {
            if (nodes.Contains(node))
            {
                nodes.Remove(node);
                edges.RemoveNode(node);
                RaiseNodeRemoved(node);
            }
        }

        public List<Node> ChildrenOf(Node node)
        {
            return edges.ChildrenOf(node);
        }
        public List<Node> ParentsOf(Node node)
        {
            return edges.ParentsOf(node);
        }

        #region Raisers
        private void RaiseNodeAdded(Node node)
        {
            EventHandler<NodeEventArgs> handler = NodeAdded;
            if (handler != null)
            {
                handler(this, new NodeEventArgs(node));
            }
        }

        private void RaiseNodeRemoved(Node node)
        {
            EventHandler<NodeEventArgs> handler = NodeRemoved;
            if (handler != null)
            {
                handler(this, new NodeEventArgs(node));
            }
        }
        private void RaiseEdgeAdded(Node from, Node to, string label)
        {
            EventHandler<EdgeEventArgs> handler = EdgeAdded;
            if (handler != null)
            {
                handler(this, new EdgeEventArgs(from, to, label));
            }
        }
        private void RaiseEdgeAdded(Node from, Node to)
        {
            EventHandler<EdgeEventArgs> handler = EdgeAdded;
            if (handler != null)
            {
                handler(this, new EdgeEventArgs(from, to));
            }
        }
        private void RaiseEdgeRemoved(Node from, Node to)
        {
            EventHandler<EdgeEventArgs> handler = EdgeRemoved;
            if (handler != null)
            {
                handler(this, new EdgeEventArgs(from, to));
            }
        }
        #endregion
        #endregion
    }


}
