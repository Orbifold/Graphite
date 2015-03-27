using System;
using System.Windows;

namespace Orbifold.Graphite
{
    /// <summary>
    /// A graph node.
    /// </summary>
    public sealed class Node
    {
        internal Graph Graph { get; set; }
        /// <summary>
        /// Gets the empty node.
        /// </summary>
        /// <value>The empty.</value>
        public static Node Empty
        {
            get { return new Node { Title = string.Empty, Info = string.Empty, Type = NodeType.NotSpecified }; }
        }
        #region Constructor
        ///<summary>
        ///Default constructor
        ///</summary>
        public Node()
        {
            ID = Guid.NewGuid().ToString();
        }
        #endregion

        #region Properties

        /// <summary>
        /// If <c>true</c> this node will not be moved by the layout, it will however have an effect on other nodes if linked to it.
        /// </summary>
        public bool IsFixed;

        /// <summary>
        /// Sets the initial position of the node. If used with the <see cref="IsFixed"/> property this will hold the node in place at the specified location while still participating 
        /// in the layout process (i.e. have an effect on the linked nodes).
        /// </summary>
        public Point InitialPosition;
        /// <summary>
        /// An identifier, by default this is set to a Guid.
        /// </summary>
        public string ID;

        /// <summary>
        /// The node type
        /// </summary>
        public NodeType Type;
        /// <summary>
        /// The text appearing in the diagram.
        /// </summary>
        public string Title;
        /// <summary>
        /// The info you want to attach to the node.
        /// </summary>
        public object Info; 
        #endregion

    }
}