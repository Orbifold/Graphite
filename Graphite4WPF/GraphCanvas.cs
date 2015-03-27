
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;
using System.Xml.Linq;

namespace Orbifold.Graphite
{

    /// <summary>
    /// This is the diagramming control you can embed in your WPF application.
    /// </summary>
    public class GraphCanvas : Canvas
    {
        #region Events

        /// <summary>
        /// Occurs when the mouse is hovering a node.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeHovering;
        /// <summary>
        /// Occurs when a node is clicked.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeClick;
        /// <summary>
        /// Occurs when a node has been deleted.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeDelete;
        /// <summary>
        /// Occurs when a node was added.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeAdd;
        /// <summary>
        /// Occurs when the first node of a pair was slected.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeSelect1;
        /// <summary>
        /// Occurs when the second node of a pair was selected.
        /// </summary>
        public event EventHandler<NodeEventArgs> NodeSelect2;
        /// <summary>
        /// Occurs when an edge was added.
        /// </summary>
        public event EventHandler<EdgeEventArgs> EdgeAdd;
        /// <summary>
        /// Occurs when an edgde was deleted.
        /// </summary>
        public event EventHandler<EdgeEventArgs> EdgeDelete;

        #endregion

        #region Constants

        private const double nodeHeight = 27D;
        private const double lineThickness = 1D;
        private const double attractionStrength = 0.9D;
        private const double repulsionStrength = 1200D;
        private const double centroidSpeed = 2D;
        private const double timeStep = 0.95;
        private const double damping = 0.90;
        private const double lineHighlightThickness = 2D;
        private const double repulsionClipping = 200D;

        #endregion

        #region Fields
        private DispatcherTimer timer;
        private int bubbleAmount = 15;
        private NodeDragState dragState;
        private readonly Random rnd = new Random();
        private readonly DispatcherTimer layoutTimer;
        private readonly Graph graph;
        private readonly Dictionary<Node, NodeState> nodeStates;
        private readonly Brush nodeBackground;
        private readonly Brush nodeDefaultBorder;
        private readonly Brush nodeHighlightBorder;
        private readonly Brush nodeSelectedBorder;
        public Brush LineBrush { get; set; }
        private double currentZoom = 1D;
        #endregion

        #region Properties

        #region IsHomeDeletionEnabled

        /// <summary>
        /// IsHomeDeletionEnabled Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsHomeDeletionEnabledProperty =
            DependencyProperty.Register("IsHomeDeletionEnabled", typeof(bool), typeof(GraphCanvas),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the IsHomeDeletionEnabled property.  This dependency property 
        /// indicates whether the canvas home node can be deleted. 
        /// </summary>
        public bool IsHomeDeletionEnabled
        {
            get { return (bool)GetValue(IsHomeDeletionEnabledProperty); }
            set { SetValue(IsHomeDeletionEnabledProperty, value); }
        }

        #endregion

        public bool UseCentralForce { get; set; }

        /// <summary>
        /// Gets the first selected node of the node pair which allows you to add a new edge through the <see cref="AddEdgeToSelected"/> method.
        /// </summary>
        /// <value>The selected node.</value>
        public Node Selected1 { get; internal set; }
        /// <summary>
        /// Gets the second selected node of the node pair which allows you to add a new edge through the <see cref="AddEdgeToSelected"/> method.
        /// </summary>
        /// <value>The selected node.</value>
        public Node Selected2 { get; internal set; }

        internal bool ReadOnly { get; set; }
        /// <summary>
        /// Gets or sets the home node.
        /// </summary>
        /// <value>The home.</value>
        public Node Home { get; set; }
        internal bool BlockEvent { get; set; }
        /// <summary>
        /// Gets the graph or model on which the diagram is based.
        /// </summary>
        /// <value>The graph.</value>
        internal Graph Graph
        {
            get { return graph; }

        }
        /// <summary>
        /// Gets the nodes from the graph.
        /// </summary>
        /// <value>A read-only collection of the graph nodes. To add or delete nodes use the appropriate methods instead of this collection.</value>
        public ReadOnlyCollection<Node> Nodes
        {
            get { return new ReadOnlyCollection<Node>(graph.Nodes.ToList()); }
        }
        /// <summary>
        /// Returns a random node from the graph.
        /// </summary>
        /// <value>The random node.</value>
        public Node RandomNode
        {
            get
            {
                return graph.RandomNode;

            }
        }

        /// <summary>
        /// Gets the center of gravity of the diagram.
        /// </summary>
        /// <value>The center of gravity.</value>
        public Point CenterOfGravity
        {
            get
            {
                if (NodeStates.Count == 0) return new Point(0, 0);
                var centroid = new Point();

                foreach (var state in NodeStates.Values)
                {
                    centroid.X += state.Position.X;
                    centroid.Y += state.Position.Y;
                }
                centroid.X = centroid.X / NodeStates.Count;
                centroid.Y = centroid.Y / NodeStates.Count;
                return centroid;
            }
        }

        protected Dictionary<Node, NodeState> NodeStates
        {
            get { return nodeStates; }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphCanvas"/> class.
        /// </summary>
        public GraphCanvas()
        {
            Loaded += GraphCanvas_Loaded;
            graph = new Graph();
            graph.NodeAdded += graph_NodeAdded;
            graph.EdgeAdded += graph_EdgeAdded;
            graph.EdgeRemoved += graph_EdgeRemoved;
            graph.NodeRemoved += graph_NodeRemoved;
            UseCentralForce = true;

            nodeStates = new Dictionary<Node, NodeState>();
            layoutTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 10) };
            layoutTimer.Tick += TimerTick;

            //if (Application.Current != null)
            //{
            //    //nodeBackground = Application.Current.Resources["NodeBackground"] as LinearGradientBrush;
            //    //lineBrush = Application.Current.Resources["EdgeStroke"] as LinearGradientBrush;
            //    //nodeDefaultBorder = Application.Current.Resources["NodeBorder"] as SolidColorBrush;
            //    //nodeHighlightBorder = Application.Current.Resources["NodeHighlightBorder"] as SolidColorBrush;
            //    //nodeSelectedBorder = Application.Current.Resources["NodeSelectedBorder"] as SolidColorBrush;

            //}
            //else
            {
                MouseMove += Canvas_MouseMove;
                MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
                MouseLeave += Canvas_MouseLeave;
                MouseLeftButtonDown += GraphCanvas_MouseLeftButtonDown;
                nodeBackground = Brushes.White;
                nodeSelectedBorder = nodeHighlightBorder = nodeDefaultBorder = LineBrush = Brushes.Black;
            }


        }



        /// <summary>
        /// Handles the MouseLeftButtonDown event of the GraphCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void GraphCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (Selected1 != null)
                {
                    NodeStates[Selected1].Visual.BorderBrush = nodeDefaultBorder;
                    Selected1 = null;
                }
                if (Selected2 != null)
                {
                    NodeStates[Selected2].Visual.BorderBrush = nodeDefaultBorder;
                    Selected2 = null;
                }
            }
        }

        /// <summary>
        /// Determines whether there is an edge between the given nodes
        /// </summary>
        /// <param name="from"> The start node.</param>
        /// <param name="to">The end node.</param>
        /// <returns>
        /// 	<c>true</c> if the specified nodes have an edge in the current graph; otherwise, <c>false</c>.
        /// </returns>
        public bool HasEdge(Node from, Node to)
        {
            return graph.HasEdge(from, to);
        }
        /// <summary>
        /// Determines whether the specified node is part of the current graph.
        /// </summary>
        /// <param name="node">Some node.</param>
        /// <returns>
        /// 	<c>true</c> if the specified node is part of the graph; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsNode(Node node)
        {
            return graph.Nodes.Contains(node);
        }

        /// <summary>
        /// Handles the Loaded event of the GraphCanvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void GraphCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            StartLayout();
        }

        /// <summary>
        /// Adds the home node.
        /// </summary>
        private void AddHomeNode()
        {
            var home = new Node { Type = NodeType.Standard, Title = "Home" };
            AddNode(home);
            Home = home;
        }

        private void BubbleNodes()
        {
            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500) };
            timer.Tick += timer_Tick;

            timer.Start();
        }

        public delegate void testje(Node source);
        void timer_Tick(object sender, EventArgs e)
        {
            if (Graph.Nodes.Count < bubbleAmount)
                Dispatcher.Invoke(new testje(AttachRandomNodeTo), new object[] { null });
            //Dispatcher.BeginInvoke(DispatcherPriority.Render,
            //                       Delegate.CreateDelegate(typeof(testje),
            //                                               this,this.GetType().GetMethod("AttachRandomNodeTo")),(Node)null);
            else
                timer.Stop();
        }
        /// <summary>
        /// Adds some bubbling nodes to the diagram.
        /// </summary>
        /// <see cref="AddRandomNodes"/>
        /// <param name="amount">The amount of nodes to add.</param>
        public void AddBubblingNodes(int amount)
        {
            bubbleAmount = amount;
            BubbleNodes();
        }

        /// <summary>
        /// Adds some random nodes to the diagram.
        /// </summary>
        /// <seealso cref="AddBubblingNodes"/>
        /// <param name="amount">The amount of nodes to add.</param>
        public void AddRandomNodes(int amount)
        {
            ReadOnly = true;
            for (var i = 0; i < amount; i++)
            {
                AttachRandomNodeTo(null);
            }
            ReadOnly = false;
        }

        public void LoadSampleGraph()
        {
            StopLayout();
            NewDiagram(false);
            BuildGraph(GetType().Assembly.GetManifestResourceStream("Graphite.SampleGraph.txt"));
            //XDocument xdoc = XDocument.Load(GetType().Assembly.GetManifestResourceStream("Graphite.SampleGraph.xml"));
            //BuildGraph(xdoc);


            StartLayout();
        }

        /// <summary>
        /// Loads data from a network location.
        /// <para></para>
        /// <para>            </para>
        /// <para>           
        /// graphite.Load(&quot;http://www.orbifold.net/test.txt&quot;);</para>
        /// </summary>
        /// <remarks>
        /// Very, very important, read the <see
        /// cref="http://msdn.microsoft.com/en-us/library/cc645032(VS.95).aspx">security
        /// access restriction in Silverlight</see>. You need to add the
        /// <b>ClientAccessPolicy.xml</b> file in the root of the server in order to give
        /// Silverlight access to the file you want to read. Something like:
        /// <para>             </para>
        /// <para>         <c>    &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;</c></para>
        /// <code>              &lt;access-policy&gt;
        ///                 &lt;cross-domain-access&gt;
        ///                   &lt;policy &gt;
        ///                     &lt;allow-from&gt;
        ///                       &lt;domain uri=&quot;*&quot;/&gt;
        ///                     &lt;/allow-from&gt;
        ///                     &lt;grant-to&gt;
        ///                       &lt;resource path=&quot;/&quot; include-subpaths=&quot;false&quot;/&gt;
        ///                     &lt;/grant-to&gt;
        ///                  &lt;/policy&gt;
        ///                 &lt;/cross-domain-access&gt;
        ///               &lt;/access-policy&gt;</code>
        /// </remarks>
        /// <param name="networkpath"></param>
        public void LoadGraphFromFlatFile(string networkpath)
        {
            StopLayout();
            NewDiagram(false);
            var wc = new WebClient();
            wc.DownloadStringAsync(new Uri(networkpath));
            wc.DownloadStringCompleted += wc_DownloadStringCompleted;
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null) return;

            var encoding = new System.Text.UTF8Encoding();
            var ms = new MemoryStream(encoding.GetBytes(e.Result));
            BuildGraph(ms);
            StartLayout();

        }


        /// <summary>
        /// Clears the diagram.
        /// </summary>
        /// <param name="addHomeNode">if set to <c>true</c> the home node will be added. </param>
        public void NewDiagram(bool addHomeNode)
        {
            graph.Clear();
            NodeStates.Clear();
            Children.Clear();
            if (addHomeNode)
                AddHomeNode();
        }
        private void graph_EdgeRemoved(object sender, EdgeEventArgs e)
        {
            RemoveVisualEdge(e.From, e.To);
            RaiseEdgeEvent(new Edge(e.From, e.To), EdgeDelete);
        }

        /// <summary>
        /// Removes the visual edge corresponding to the given nodes.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        private void RemoveVisualEdge(Node from, Node to)
        {
            //note that the parentlines and childrenlines are keyed by the from-node

            //try to find the edge in the parent collection of the from-node
            var found = new KeyValuePair<Node, VisualEdge>();
            foreach (var line in NodeStates[from].ParentLines.Where(line => line.Key == to))
            {
                found = line;
                break;
            }
            if (found.Key != null)
            {
                NodeStates[from].ParentLines.Remove(found);
                NodeStates[to].ChildrenLines.Remove(found);
                Children.Remove(found.Value);
            }
            //try to find the edge in the parent collection of the to-node
            found = new KeyValuePair<Node, VisualEdge>();
            foreach (var line in NodeStates[to].ParentLines.Where(line => line.Key == from))
            {
                found = line;
                break;
            }
            if (found.Key != null)
            {
                NodeStates[to].ParentLines.Remove(found);
                NodeStates[from].ChildrenLines.Remove(found);
                Children.Remove(found.Value);
            }
        }

        void graph_NodeRemoved(object sender, NodeEventArgs e)
        {
            foreach (var line in NodeStates[e.Node].ParentLines)
            {
                Children.Remove(line.Value);
                var kvp = NodeStates[line.Key].ChildrenLines.SingleOrDefault(s => s.Key == e.Node);
                NodeStates[line.Key].ChildrenLines.Remove(kvp);
            }
            foreach (var line in NodeStates[e.Node].ChildrenLines)
            {
                Children.Remove(line.Value);
                var kvp = NodeStates[line.Key].ParentLines.SingleOrDefault(s => s.Key == e.Node);
                NodeStates[line.Key].ParentLines.Remove(kvp);
            }
            Children.Remove(NodeStates[e.Node].Visual);
            NodeStates.Remove(e.Node);
            RaiseNodeEvent(e.Node, NodeDelete);
        }



        /// <summary>
        /// Creates and attaches a random node to the given node.
        /// </summary>
        /// <param name="source">The node to which the new node will be attached. If <c>null</c> a random node from the diagram will be chosen.</param>
        public void AttachRandomNodeTo(Node source)
        {
            var content = Utils.RandomTitle();
            var n = new Node { Title = content.Title, Info = content.Info };
            AddNode(n);
            if (graph.Nodes.Count > 0)
            {
                Node b;
                if (source == null)
                {
                    b = graph.Nodes[rnd.Next(0, graph.Nodes.Count)];
                    while (b == n)
                    {
                        b = graph.Nodes[rnd.Next(0, graph.Nodes.Count)];
                    }
                }
                else
                {
                    b = source;
                }
                AddEdge(n, b);
            }


        }

        void graph_EdgeAdded(object sender, EdgeEventArgs e)
        {
            if (BlockEvent) return;
            CreateVisualForEdge(e.From, e.To, e.Label);
            RaiseEdgeEvent(e.From, e.To, EdgeAdd);
        }

        void graph_NodeAdded(object sender, NodeEventArgs e)
        {
            if (BlockEvent) return;
            var ns = AddNodeState(e.Node);
            AddVisualNode(new KeyValuePair<Node, NodeState>(e.Node, ns));



        }
        #endregion

        #region Mehtods

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseWheel"/> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.MouseWheelEventArgs"/> that contains the event data.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            //the 'zoom' 
            var source = e.Source as GraphCanvas;

            if (source == null) return;
            currentZoom += Math.Sign(e.Delta) * 0.2D;
            currentZoom = Math.Min(Math.Max(currentZoom, 0.2D), 2D);
            var st = new ScaleTransform(currentZoom, currentZoom, ActualWidth / 2, ActualHeight / 2);
            LayoutTransform = st;
        }

        #region Raisers
        /// <summary>
        /// Raises node events.
        /// </summary>
        /// <param name="node">The node information.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <returns></returns>
        private NodeEventArgs RaiseNodeEvent(Node node, EventHandler<NodeEventArgs> eventHandler)
        {
            var handler = eventHandler;
            if (handler != null)
            {
                var arg = new NodeEventArgs(node);
                handler(this, arg);
                return arg;
            }
            return null;
        }
        /// <summary>
        /// Raises edge events.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <returns></returns>
        private void RaiseEdgeEvent(Edge edge, EventHandler<EdgeEventArgs> eventHandler)
        {
            var handler = eventHandler;
            if (handler != null)
            {
                var arg = new EdgeEventArgs(edge);
                handler(this, arg);
                return;
            }
            return;
        }
        private void RaiseEdgeEvent(Node from, Node to, EventHandler<EdgeEventArgs> eventHandler)
        {
            RaiseEdgeEvent(new Edge(from, to), eventHandler);
        }

        #endregion
        /// <summary>
        /// Code ran when the layout timer ticks
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void TimerTick(object sender, EventArgs e)
        {
            StepLayout();
        }
        /// <summary>
        /// Starts the layout.
        /// </summary>
        public void StartLayout()
        {
            if (!layoutTimer.IsEnabled)
                layoutTimer.Start();
        }
        /// <summary>
        /// Starts the layout with the specified timer interval.
        /// </summary>
        /// <param name="millisecs">The timer's interval in milliseconds.</param>
        public void StartLayout(int millisecs)
        {
            if (millisecs <= 10)
            {
                throw new ApplicationException("The timer value should be bigger than ten milliseconds.");
            }
            layoutTimer.Interval = TimeSpan.FromMilliseconds(millisecs);
            StartLayout();
        }

        /// <summary>
        /// Starts the layout for a specified time span.
        /// </summary>
        /// <param name="duration">The duration during which the layout process will act.</param>
        public void StartLayout(TimeSpan duration)
        {
            var timeWindowTimer = new DispatcherTimer { Interval = duration };
            timeWindowTimer.Tick += (sender, args) =>
                              {
                                  StopLayout();
                                  timeWindowTimer.Stop();
                              };
            StartLayout();
            timeWindowTimer.Start();
        }

        /// <summary>
        /// Stops the layout.
        /// </summary>
        public void StopLayout()
        {
            if (layoutTimer.IsEnabled)
                layoutTimer.Stop();
        }

        /// <summary>
        /// Adds a node to the diagram.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <seealso cref="AddEdge">AddEdge</seealso>
        public void AddNode(Node node)
        {
            RaiseNodeEvent(node, NodeAdd);
            graph.AddNode(node);
        }
        /// <summary>
        /// Adds a node and an edge from this node to the given target.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="to"></param>
        public void AddNode(Node node, Node to)
        {
            if (node == null || to == null)
            {
                throw new ArgumentNullException("One of the node argument was NULL.");
            }
            AddNode(node);
            AddEdge(node, to);
        }

        /// <summary>
        /// Adds an edge between the two given nodes.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public void AddEdge(Node from, Node to)
        {
            AddEdge(from, to, "");
        }
        public void AddEdge(Node from, Node to, string label)
        {
            if (from == null || to == null)
            {
                return;
            }

            if (!HasEdge(from, to))
                graph.AddEdge(from, to, label);
        }

        /// <summary>
        /// Displays the underlying graph. Use this method after data was batch imported in the <see cref="Graph"/>.
        /// </summary>
        internal void DisplayGraph()
        {


            foreach (var n in graph.Nodes)
            {
                AddNodeState(n);
            }


            foreach (var nodeLoc in NodeStates)
            {
                AddVisualNode(nodeLoc);
            }
            foreach (var edge in graph.Edges)
            {
                CreateVisualForEdge(edge.From, edge.To, "");
            }


        }

        private void AddVisualNode(KeyValuePair<Node, NodeState> nodeLoc)
        {
            var visual = CreateVisualForNode(nodeLoc.Key);
            nodeLoc.Value.Visual = visual;
            visual.NodeState = nodeLoc.Value;

            /* Set the position of the node */
            visual.SetValue(LeftProperty, nodeLoc.Value.Position.X - visual.ActualWidth / 2);
            visual.SetValue(TopProperty, nodeLoc.Value.Position.Y - visual.ActualHeight / 2);

            Children.Add(visual);


        }
        protected virtual void CreateVisualForEdge(Node from, Node to, string label)
        {
            if (HasVisualEdge(from, to)) return;

            var fromState = NodeStates[from];
            var toState = NodeStates[to];

            var a = fromState.Position;
            var b = toState.Position;



            var edge = new VisualEdge
                           {
                               X1 = a.X,
                               X2 = b.X,
                               Y1 = a.Y,
                               Y2 = b.Y,
                               Stroke = LineBrush,
                               StrokeThickness = lineThickness,
                               Label = label
                           };
            if (fromState.ChildrenLines.Count(s => s.Key == to) == 0)
                fromState.ChildrenLines.Add(new KeyValuePair<Node, VisualEdge>(to, edge));
            if (toState.ChildrenLines.Count(s => s.Key == from) == 0)
                toState.ParentLines.Add(new KeyValuePair<Node, VisualEdge>(from, edge));

            Children.Insert(0, edge); //insert makes sure the edge is underneath all nodes
            if (edge.EdgeLabel != null) Children.Insert(0, edge.EdgeLabel);

ToolTipService.SetToolTip(edge, new Tip { From = from, To = to });
//edge.MouseEnter += (sender, args) => { MessageBox.Show("Whatever you wish to display about the connection or nodes"); };

        }

        protected bool HasVisualEdge(Node from, Node to)
        {
            var fromState = NodeStates[from];
            var toState = NodeStates[to];

            return fromState.ChildrenLines.Count(s => s.Key == to) > 0 || fromState.ParentLines.Count(s => s.Key == to) > 0 || toState.ChildrenLines.Count(s => s.Key == from) > 0 || toState.ParentLines.Count(s => s.Key == from) > 0;
        }


        /// <summary>
        /// Adds a state for the given node.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private NodeState AddNodeState(Node n)
        {
            if (n == null) return null;
            Point initialPosition;
            if (n.InitialPosition.X != 0 || n.InitialPosition.Y != 0)
                initialPosition = n.InitialPosition;
            else
                initialPosition = new Point(300 + 200 * (rnd.NextDouble() - 0.5), 300 + 150 * (rnd.NextDouble() - 0.5));
            var ns = new NodeState
                                {
                                    Position = initialPosition,
                                    Velocity = new Point(0, 0),
                                    Node = n
                                };
            NodeStates.Add(n, ns);
            return ns;
        }

        /// <summary>
        /// Applies one step of the graph layout algorithm, moving nodes to a more stable configuration.
        /// </summary>
        public void StepLayout()
        {
            for (var i = 0; i < 10; i++)
            {
                Layout();
                if (!dragState.IsDragging && UseCentralForce)
                    MoveDiagramTo(new Point(ActualWidth / 2, ActualHeight / 2), centroidSpeed);
                UpdateVisualPositions();
            }
        }
        private void UpdateVisualPosition(NodeState ns)
        {


            if (double.IsNaN(ns.Position.X) || double.IsNaN(ns.Position.Y))
                return;

            SetLeft(ns.Visual, ns.Position.X - ns.Visual.ActualWidth / 2);
            SetTop(ns.Visual, ns.Position.Y - ns.Visual.ActualHeight / 2);
            //todo: use databinding here
            foreach (var kvp in ns.ChildrenLines)
            {
                var childLoc = NodeStates[kvp.Key].Position;
                kvp.Value.X1 = ns.Position.X;
                kvp.Value.Y1 = ns.Position.Y;
                kvp.Value.X2 = childLoc.X;
                kvp.Value.Y2 = childLoc.Y;
                var p = GetLabelPosition(kvp.Value);
                SetLeft(kvp.Value.EdgeLabel, p.X);
                SetTop(kvp.Value.EdgeLabel, p.Y);
            }

        }

        private static Point GetLabelPosition(VisualEdge edge)
        {
            return new Point(GetHalfWay(edge.X1, edge.X2), GetHalfWay(edge.Y1, edge.Y2));
        }
        private static double GetHalfWay(double a, double b)
        {
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return 0;
            }
            if (a < b)
                return a + (b - a) / 2;
            return b + (a - b) / 2;
        }

        private void UpdateVisualPositions()
        {
            foreach (var nodeLoc in NodeStates)
            {
                UpdateVisualPosition(nodeLoc.Value);
            }
        }

        /// <summary>
        /// Moves the diagram globally to a given location.
        /// </summary>
        /// <param name="point">The desired centroid.</param>
        /// <param name="speed">The speed with which the visual motion should occur. Around a value of 10 the motion will appear as instantenously, while a value of 0.5 gives a slow-motion effect.</param>
        private void MoveDiagramTo(Point point, double speed)
        {
            var centroid = CenterOfGravity;
            var delta = new Point(point.X - centroid.X, point.Y - centroid.Y);

            var deltaLength = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
            if (deltaLength == 0)
                return; //we're at the right spot already
            // If we're very close, don't move at the full speed anymore
            if (speed > deltaLength) speed = 0.5;

            delta.X = (delta.X / deltaLength) * speed;
            delta.Y = (delta.Y / deltaLength) * speed;

            foreach (var state in NodeStates.Values)
            {
                state.Position.X += delta.X;
                state.Position.Y += delta.Y;
            }
        }

        #region Layout algorithm

        /// <summary>
        /// See the <see
        /// cref="http://en.wikipedia.org/wiki/Li%C3%A9nard-Wiechert_Potentials">Lienard-Wiechert
        /// potential</see>.
        /// </summary>
        /// <param name="a">a point</param>
        /// <param name="b">another point</param>
        private static Point RepulsionForce(Point a, Point b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y;
            var sqDist = dx * dx + dy * dy;
            var d = Math.Sqrt(sqDist);
            var repulsion = repulsionStrength * 1.0 / sqDist;
            repulsion += -repulsionStrength * 0.00000006 * d;
            //clip the repulsion
            if (repulsion > repulsionClipping) repulsion = repulsionClipping;
            return new Point(repulsion * (dx / d), repulsion * (dy / d));
        }

        /// <summary>
        /// Calculates the attractions force between two given points.
        /// </summary>
        /// <param name="a">A point in space.</param>
        /// <param name="b">Anoter point in space.</param>
        /// <returns>The force vector.</returns>
        private static Point AttractionForce(Point a, Point b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y;
            var sqDist = dx * dx + dy * dy;
            var d = Math.Sqrt(sqDist);
            var mag = -attractionStrength * 0.001 * Math.Pow(d, 1.20);

            return new Point(mag * (dx / d), mag * (dy / d));
        }

        /// <summary>
        /// The actual spring-embedder algorithm.
        /// </summary>
        /// <returns></returns>
        protected virtual void Layout()
        {

            foreach (var kvp in NodeStates)
            {
                if (dragState.IsDragging && dragState.NodeBeingDragged == kvp.Value) continue;
                if (kvp.Value.Node.IsFixed) continue;

                var n = kvp.Key;
                var state = kvp.Value;

                var f = new Point(0, 0); // Force
                //compute the repulsion on this node, with respect to ALL nodes
                foreach (var coulomb in from kvpB in NodeStates where kvpB.Key != n select RepulsionForce(state.Position, kvpB.Value.Position))
                {
                    f.X += coulomb.X;
                    f.Y += coulomb.Y;
                }
                //compute the attraction on this node, only to the adjacent nodes
                foreach (var child in graph.AdjacentNodes(n))
                {
                    var hooke = AttractionForce(state.Position, NodeStates[child].Position);
                    f.X += hooke.X;
                    f.Y += hooke.Y;
                }

                var v = state.Velocity;

                state.Velocity = new Point((v.X + timeStep * f.X) * damping, (v.Y + timeStep * f.Y) * damping);

                state.Position.X += timeStep * state.Velocity.X;
                state.Position.Y += timeStep * state.Velocity.Y;
            }

        }

        #endregion

        #region VisualNode creation

        /// <summary>
        /// Creates the visual representation for a given node.
        /// </summary>
        /// <param name="n">The node for which a visual will be created.</param>
        /// <returns></returns>
        private VisualNode CreateVisualForNode(Node n)
        {
            var visualNode = new VisualNode { BorderBrush = nodeDefaultBorder, Title = n.Title, Info = n.Info };
            switch (n.Type)
            {
                case NodeType.Standard:
                    //use the normal template mechanism
                    visualNode.Style = TryFindResource(n.Type.ToString()) as Style;
                    break;
                case NodeType.Bubble:
                case NodeType.Person:
                case NodeType.Idea:
                    visualNode.Style = TryFindResource(n.Type.ToString()) as Style;
                    break;
                case NodeType.NotSpecified:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("n");
            }

            if (Application.Current != null)
            {
                visualNode.MouseLeftButtonDown += VisualNode_MouseLeftButtonDown;
                visualNode.MouseEnter += VisualNode_MouseEnter;
                visualNode.MouseLeave += VisualNode_MouseLeave;
                visualNode.DataContext = n.Info;
            }



            return visualNode;
        }

        void VisualNode_MouseLeave(object sender, MouseEventArgs e)
        {
            if (dragState.IsDragging) return;
            var visualNode = sender as VisualNode;

            if (visualNode == null)
                return;
            LowlightNode(visualNode);

            HighlightChildrenReset(visualNode);
            HighlightParentsReset(visualNode);
            RaiseNodeEvent(Node.Empty, NodeHovering);

        }

        void VisualNode_MouseEnter(object sender, MouseEventArgs e)
        {
            if (dragState.IsDragging) return;
            var visualNode = sender as VisualNode;
            if (visualNode == null)
                return;
            HighlightNode(visualNode);
            HighlightChildren(visualNode);
            HighlightParents(visualNode);
            RaiseNodeEvent(visualNode.NodeState.Node, NodeHovering);
        }

        /// <summary>
        /// Highlights the given node.
        /// </summary>
        /// <param name="visualNode">The visual node.</param>
        private void HighlightNode(VisualNode visualNode)
        {
            visualNode.Inflate();
            visualNode.BorderBrush = nodeHighlightBorder;
        }
        private void LowlightNode(VisualNode visualNode)
        {
            visualNode.Deflate();
            if (visualNode.NodeState.Node == Selected1 || visualNode.NodeState.Node == Selected2)
                visualNode.BorderBrush = nodeSelectedBorder;
            else
                visualNode.BorderBrush = nodeDefaultBorder;
        }
        /// <summary>
        /// Highlights the node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void HighlightNode(Node node)
        {
            if (node == null) return;
            if (NodeStates.Keys.Contains(node))
            {
                HighlightNode(NodeStates[node].Visual);
            }
        }
        /// <summary>
        /// Lowlights the given node.
        /// </summary>
        /// <seealso cref="HighlightNode"/>
        /// <param name="node">The node.</param>
        public void LowlightNode(Node node)
        {
            if (node == null) return;
            if (NodeStates.Keys.Contains(node))
            {
                LowlightNode(NodeStates[node].Visual);
            }
        }

        internal void HighlightParentsReset(VisualNode visualNode)
        {
            foreach (var line in visualNode.NodeState.ParentLines)
            {
                LowlightNode(line.Key);
                line.Value.Stroke = LineBrush;
                line.Value.StrokeThickness = lineThickness;

            }
        }
        internal void HighlightChildrenReset(VisualNode visualNode)
        {
            foreach (var line in visualNode.NodeState.ChildrenLines)
            {
                LowlightNode(line.Key);
                line.Value.Stroke = LineBrush;
                line.Value.StrokeThickness = lineThickness;

            }
        }

        internal void HighlightParents(VisualNode visualNode)
        {
            foreach (var line in visualNode.NodeState.ParentLines)
            {
                HighlightNode(line.Key);
                line.Value.Stroke = nodeHighlightBorder;
                line.Value.StrokeThickness = lineHighlightThickness;
            }
        }

        internal void HighlightChildren(VisualNode visualNode)
        {
            foreach (var line in visualNode.NodeState.ChildrenLines)
            {
                HighlightNode(line.Key);
                line.Value.Stroke = nodeHighlightBorder;
                line.Value.StrokeThickness = lineHighlightThickness;
            }
        }
        /// <summary>
        /// Highlights the children nodes of the given node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void HighlightChildren(Node node)
        {
            if (node == null) return;
            if (NodeStates.Keys.Contains(node))
                HighlightChildren(NodeStates[node].Visual);
        }
        /// <summary>
        /// Highlights the parent nodes of the given node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void HighlightParents(Node node)
        {
            if (node == null) return;
            if (NodeStates.Keys.Contains(node))
                HighlightParents(NodeStates[node].Visual);
        }
        /// <summary>
        /// Handles the MouseLeftButtonDown event of the VisualNode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        void VisualNode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ReadOnly) return;

            var visualNode = sender as VisualNode;
            var n = NodeStates.Single(s => s.Value.Visual == visualNode).Key;
            var arg = RaiseNodeEvent(n, NodeClick);
            if (arg != null && arg.Handled)
            {
                e.Handled = true;
                return;
            }
            if (n.IsFixed) return; //do not move the fixed nodes

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (Selected1 == null)
                {
                    Selected1 = n;
                    RaiseNodeEvent(n, NodeSelect1);
                    return;
                }
                if (Selected2 == null)
                {
                    Selected2 = n;
                    RaiseNodeEvent(n, NodeSelect2);
                    return;
                }
                //neither are null
                NodeStates[Selected1].Visual.BorderBrush = nodeDefaultBorder;
                NodeStates[Selected2].Visual.BorderBrush = nodeDefaultBorder;

                Selected1 = n;
                Selected2 = null;
                RaiseNodeEvent(n, NodeSelect1);

                return;
            }

            if (visualNode == null)
                return;

            dragState.NodeBeingDragged = visualNode.NodeState;
            dragState.OffsetWithinNode = e.GetPosition(visualNode);
            dragState.IsDragging = true;
        }
        /// <summary>
        /// Deletes the given node from the diagram and removes all edges linked to this node.
        /// </summary>
        /// <param name="node">The node.</param>
        public void DeleteNode(Node node)
        {
            if (!IsHomeDeletionEnabled && node.Equals(Home))
                throw new ApplicationException(Orbifold.Graphite.Properties.Resources.HomeDeletionForbidden);
            // MessageBox.Show("The home node cannot be deleted.", "Forbidden", MessageBoxButton.OK);

            graph.RemoveNode(node);
        }
        /// <summary>
        /// Deletes the edge between the two given nodes.
        /// </summary>
        public void DeleteEdge(Edge edge)
        {

        }
        /// <summary>
        /// Deletes the edge between the two given nodes.
        /// </summary>
        /// <param name="from">From node.</param>
        /// <param name="to">To node.</param>
        public void DeleteEdge(Node from, Node to)
        {
            graph.RemoveEdge(from, to);
        }

        #endregion
        #region Reading files
        /// <summary>
        /// Some file checks before load.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private bool FilePathIsOK(string path)
        {
            //if (!File.Exists(path))
            //    return false;
            ////anything else?
            return true;
        }



        /// <summary>
        /// Creates a graph by interpreting a specially formatted text file.
        /// </summary>
        /// <returns>The graph created.</returns>
        internal void BuildGraph(Stream s)
        {
            var graphLines = Utils.ReadAllLines(s);
            BuildGraph(graphLines);
        }
        internal void BuildGraph(XDocument xdoc)
        {
            if (xdoc == null) return;
            if (xdoc.Root.Name != "Graph") return;
            AddNodes(xdoc);

        }

        private void AddNodes(XDocument xdoc)
        {
            var memory = new Dictionary<string, Node>();
            foreach (var element in xdoc.Root.Elements("Node"))
            {
                try
                {
                    var n = new Node();
                    if (element.Attributes("id").Count() == 0)
                    {
                        continue; //ID is mendatory
                    }
                    n.ID = element.Attribute("id").Value;
                    if (element.Attributes("Info").Count() > 0)
                    {
                        n.Info = element.Attribute("Info").Value;
                    }
                    if (element.Attributes("Title").Count() > 0)
                    {
                        n.Title = element.Attribute("Title").Value;
                    }
                    memory.Add(n.ID, n);
                    graph.AddNode(n);
                }
                catch
                {
                    continue;
                }
            }
            foreach (var element in xdoc.Root.Elements("Node"))
            {
                try
                {
                    if (element.Attributes("Links").Count() > 0)
                    {
                        var links = element.Attribute("Links").Value.Split(',');
                        var id = element.Attribute("id").Value;
                        foreach (var s in links)
                        {

                            graph.AddEdge(memory[id], memory[s]);
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

        }

        /// <summary>
        /// Builds a graph represented by the string array encoding.
        /// </summary>
        /// <param name="elements">String array from the graph API.</param>
        internal void BuildGraph(string[] elements)
        {
            //BlockEvent = true;
            var nodeDict = new Dictionary<long, Node>();

            // First pass, get all the nodes
            foreach (var node in elements)
            {
                var nodeData = node.Split('/');

                if (nodeData.Length < 2) continue;

                var n = new Node
                {
                    Title = nodeData[1]
                };

                if (nodeData.Length <= 4)
                {
                    n.Type = NodeType.Standard;
                }
                else if (nodeData[4] == "Standard")
                {
                    n.Type = NodeType.Standard;
                }
                else
                {
                    n.Type = NodeType.Bubble;
                }

                nodeDict.Add(long.Parse(nodeData[0]), n);
                graph.AddNode(n);
            }

            // Second pass, get the edges
            foreach (var node in elements)
            {
                var nodeData = node.Split('/');

                var nodeId = long.Parse(nodeData[0]);

                if (nodeData.Length > 2)
                {
                    var nodeChildren = nodeData[2].Split(',');
                    foreach (var child in nodeChildren)
                    {
                        if (child.Length == 0) continue;
                        graph.AddEdge(nodeDict[nodeId], nodeDict[long.Parse(child)]);
                    }
                }
            }
            //BlockEvent = false;

        }

        /// <summary>
        /// For testing purposes: Builds a random graph with N nodes.
        /// </summary>
        /// <param name="N">The N.</param>
        /// <returns>
        /// The random graph as represented by the local graph library.
        /// </returns>
        void CreateRandomGraph(int N)
        {
            return;

            Graph g = new Graph();

            List<Node> allNodes = new List<Node>();

            /* Create N nodes */
            for (int i = 0; i < N; i++)
            {
                Node n = new Node { Type = NodeType.Standard, Title = i.ToString() };
                allNodes.Add(n);
                g.AddNode(n);
            }


            Random r = new Random();
            for (int i = 0; i < N; i++)
            {
                int dest = r.Next(allNodes.Count);
                if (dest == i) continue;

                g.AddEdge(allNodes[i], allNodes[dest]);
            }

            return;
        }

        #endregion
        #region Canvas interaction
        /// <summary>
        /// Handles the MouseLeave event of the Canvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            dragState.IsDragging = false;
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event of the Canvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragState.IsDragging = false;
        }

        /// <summary>
        /// Handles the MouseMove event of the Canvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragState.IsDragging)
            {
                var position = e.GetPosition(dragState.NodeBeingDragged.Visual);
                position.X += (-dragState.OffsetWithinNode.X);
                position.Y += (-dragState.OffsetWithinNode.Y);

                var ns = dragState.NodeBeingDragged;

                ns.Position.X += position.X;
                ns.Position.Y += position.Y;

                SetLeft(ns.Visual, ns.Position.X - ns.Visual.ActualWidth / 2);
                SetTop(ns.Visual, ns.Position.Y - ns.Visual.ActualHeight / 2);
                if (!layoutTimer.IsEnabled)
                {
                    UpdateVisualPositions();
                }
            }
        }
        #endregion


        /// <summary>
        /// Adds a new edge between the <see cref="Selected1"/> and <see cref="Selected2"/> nodes.
        /// </summary>
        public void AddEdgeToSelected()
        {
            if (Selected1 == null || Selected2 == null)
            {
                return;
            }

            if (!HasEdge(Selected1, Selected2))
            {
                AddEdge(Selected1, Selected2);
            }
        }
        #endregion
    }

public sealed class Tip
{
    public Node From { get; set; }
    public Node To { get; set; }
}
}
