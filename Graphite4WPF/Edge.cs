using System.Windows.Shapes;

namespace Orbifold.Graphite
{
    /// <summary>
    /// Encapsulating class for an edge.
    /// </summary>
    /// <remarks>
    /// An edge is being converted to a <see cref="Line">Line </see>on the presentation
    /// level.
    /// </remarks>
    public class Edge
    {
        public Node From;
        public Node To;

        #region Construtor

        public Edge()
        {
            
        }
        public Edge(Node from, Node to)
        {
            From = from;
            To = to;
        }

        #endregion
    }
}