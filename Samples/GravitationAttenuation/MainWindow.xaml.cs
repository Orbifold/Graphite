using System;
using System.Windows;
using System.Windows.Media;
using Graphite;

namespace GravitationAttenuation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ScaleTransform CanvasScaleTransform;
        public MainWindow()
        {
            InitializeComponent();
            diagram.UseCentralForce = false;
            ZoomSlider.Value = 1.5;
            diagram.NewDiagram(false);
            for (int i = 0; i < 20; i++)
            {
                if (i == 0)
                    diagram.AddNode(new Node { Title = "Node " + i, InitialPosition = new Point(1, 0), IsFixed = true });
                else if (i == 1)
                    diagram.AddNode(new Node { Title = "Node " + i, InitialPosition = new Point(500, 0), IsFixed = true });
                else
                    diagram.AddNode(new Node { Title = "Node " + i });

            }


            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            CanvasScaleTransform = diagram.RenderTransform as ScaleTransform;
            //diagram.UseCentralForce = false;
            ZoomSlider.Value = 0.51;
            UpdateZoom();
        }

        /// <summary>
        /// Handles the OnValueChanged event of the ZoomSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="double"/> instance containing the event data.</param>
        private void ZoomSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ZoomSlider == null) return;

            UpdateZoom();
        }

        /// <summary>
        /// Updates the zoom between the slider and the flow surface.
        /// </summary>
        private void UpdateZoom()
        {
            if (CanvasScaleTransform != null)
            {
                CanvasScaleTransform.ScaleX = ZoomSlider.Value;
                CanvasScaleTransform.ScaleY = ZoomSlider.Value;
            }
            if (ZoomLabel != null)
                ZoomLabel.Text = string.Format("{0}%", Math.Round(ZoomSlider.Value * 100));

        }
    }
}
