using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Orbifold.Graphite
{
    /// <summary>
    /// The visual representation of a <see cref="Node"/>.
    /// </summary>
    public sealed class VisualNode : Control
    {

        #region Fields
        private const double InflateRatio = 1.5D;
        internal NodeState NodeState { get; set; }
        #endregion

        #region Title dependency property

        /// <summary>
        /// Title Dependency Property
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(VisualNode));

        /// <summary>
        /// Gets or sets the Title property.  
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualNode"/> class.
        /// </summary>
        public VisualNode()
        {
            DefaultStyleKey = typeof(VisualNode);
        }

        #region Info

        /// <summary>
        /// Info Dependency Property
        /// </summary>
        public static readonly DependencyProperty InfoProperty =
            DependencyProperty.Register("Info", typeof(object), typeof(VisualNode), null);

        /// <summary>
        /// Gets or sets the Info property.  This dependency property 
        /// indicates ....
        /// </summary>
        public object Info
        {
            get { return GetValue(InfoProperty); }
            set { SetValue(InfoProperty, value); }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Inflates the node.
        /// </summary>
        public void Inflate()
        {
            RenderTransform = new ScaleTransform();
            RenderTransformOrigin = new Point(0, 0);
            (RenderTransform as ScaleTransform).AnimateTo(300, InflateRatio, InflateRatio, null);
        }
        /// <summary>
        /// Deflates the node.
        /// </summary>
        public void Deflate()
        {
            RenderTransform = new ScaleTransform();
            RenderTransformOrigin = new Point(0, 0);
            (RenderTransform as ScaleTransform).AnimateTo(300, 1D, 1D, null);
        }
        #endregion




    }

}