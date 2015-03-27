using System;
using System.Collections.Generic;
using System.Windows;
using Orbifold.Graphite;

namespace Layout
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();
            diagram.Loaded += diagram_Loaded;
        }

        void diagram_Loaded(object sender, RoutedEventArgs e)
        {
            diagram.NewDiagram(false);
            diagram.StopLayout();
            CreateRandomGraph(5);
            // note that the layout method is overriden below and the GraphCanvas is not the default one.
            diagram.StartLayout(TimeSpan.FromSeconds(3));

        }
        /// <summary>
        /// Creates a random graph with the given amount of nodes (vertices).
        /// </summary>
        /// <param name="N">The amount of nodes in the random graph.</param>
        void CreateRandomGraph(int N)
        {
            var allNodes = new List<Node>();

            /* Create N nodes */
            for (int i = 0; i < N; i++)
            {
                var n = new Node { Type = NodeType.Standard, Title = "Node " + i };
                allNodes.Add(n);
                diagram.AddNode(n);
            }

            var r = new Random();
            for (int i = 0; i < N; i++)
            {
                int dest = r.Next(allNodes.Count);
                if (dest == i) continue;

                diagram.AddEdge(allNodes[i], allNodes[dest]);
            }
        }

    }
    public class MyCanvas : GraphCanvas
    {
        protected override void Layout()
        {
            var rnd = new Random();
            foreach (KeyValuePair<Node, NodeState> kvp in NodeStates)
            {
                kvp.Value.Position = new Point(rnd.Next(20, 500), rnd.Next(20, 500));
            }
        }
    }
}
