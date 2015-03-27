using System;
using System.Windows;
using System.Windows.Media;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        private ScaleTransform CanvasScaleTransform;
        public Window1()
        {
            InitializeComponent();
            diagram.NewDiagram(true);
            diagram.AddRandomNodes(25);
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
