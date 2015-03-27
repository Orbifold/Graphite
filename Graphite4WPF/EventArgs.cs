using System;

namespace Orbifold.Graphite
{
    /// <summary>
    /// Event argument to pass on node information.
    /// </summary>
    public sealed class NodeEventArgs : EventArgs
    {
        public bool Handled;
        public Node Node { get; internal set; }
        public NodeEventArgs(Node node)
        {
            Node = node;
        }
    }
    /// <summary>
    /// Event argumen to pass on edge information.
    /// </summary>
    public sealed class EdgeEventArgs : EventArgs
    {
        public Node From { get; set; }
        public Node To { get; set; }
        public string Label { get; set; }
        public EdgeEventArgs(Node from, Node to, string label) : this(from,to)
        {
            Label = label;
        }
        public EdgeEventArgs(Node from, Node to)
        {
            From = from;
            To = to;
            Label = "";
        }
        public EdgeEventArgs(Edge edge)
        {
            From = edge.From;
            To = edge.To;
            Label = "";
        }
    }
    
}