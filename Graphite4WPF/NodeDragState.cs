using System.Windows;

namespace Orbifold.Graphite
{
    /// <summary>
    /// Keeps track of what is begin dragged.
    /// </summary>
    internal struct NodeDragState
    {
        public bool IsDragging;
        public NodeState NodeBeingDragged;
        public Point OffsetWithinNode;
    };
}