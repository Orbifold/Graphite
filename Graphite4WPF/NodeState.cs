using System.Collections.Generic;
using System.Windows;

namespace Orbifold.Graphite
{
/// <summary>
/// Helper class to store visual and physical description for the nodes.
/// </summary>
public sealed class NodeState
{
    public Node Node;
    public Point Position;
    public Point Velocity;
    public VisualNode Visual;
    public List<KeyValuePair<Node, VisualEdge>> ChildrenLines = new List<KeyValuePair<Node, VisualEdge>>();
    public List<KeyValuePair<Node, VisualEdge>> ParentLines = new List<KeyValuePair<Node, VisualEdge>>();
}

   
  
}