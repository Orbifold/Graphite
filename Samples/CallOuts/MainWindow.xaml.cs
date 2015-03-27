using System;
using System.Windows;
using System.Windows.Media;
using Orbifold.Graphite;

namespace CallOuts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            diagram.NewDiagram(false);
            diagram.StopLayout();
            diagram.LineBrush = Brushes.OrangeRed;
            diagram.UseCentralForce = false;

            AddCallout(new GeoInfo { Title = "Brussels", Address = "Grand Place", Details = "City hall", Image = ResourceHelper.GetBitmap("/Resources/BigMarket.png", "CallOuts") }, new Point(240, 307));
            AddCallout(new GeoInfo { Title = "Leuven", Address = "St-Peter Church", Details = "Just in front of the city hall", Image = ResourceHelper.GetBitmap("/Resources/Leuven.png", "CallOuts") }, new Point(634, 240));
            AddCallout(new GeoInfo { Title = "Atomium", Address = "Eeuwfeestlaan 214", Details = "Expo 1958 monument", Image = ResourceHelper.GetBitmap("/Resources/Atomium.png", "CallOuts") }, new Point(281, 292));
            AddCallout(new GeoInfo { Title = "Sheraton", Address = "Keizerslaan 81", Details = "Sheraton Hotel", Image = ResourceHelper.GetBitmap("/Resources/Sheraton.png", "CallOuts") }, new Point(255, 281));

            diagram.StartLayout(TimeSpan.FromSeconds(10));
        }

        private void AddCallout(GeoInfo info, Point position)
        {
            //note that the templates should be defined in the App.xaml file and that the NodeType defines the key of the template.
            //we use the person type here as anchor
            var anchor1 = new Node { Type = NodeType.Person, IsFixed = true, InitialPosition = position };
            diagram.AddNode(anchor1);
            var callout1 = new Node { Info = info, Type = NodeType.Standard };
            diagram.AddNode(callout1);
            diagram.AddEdge(callout1, anchor1);
        }
    }
    public class GeoInfo
    {
        public string Title { get; set; }
        public string Address { get; set; }
        public string Details { get; set; }
        public ImageSource Image { get; set; }
    }

}
