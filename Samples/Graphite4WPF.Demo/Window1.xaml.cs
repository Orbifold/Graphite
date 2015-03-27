using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;
using Orbifold.Graphite;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void LoadSample()
        {
           
            var importer = new GraphImporter(graphite);
            importer.Load();
           
        }

        private void Window1_OnLoaded(object sender, RoutedEventArgs e)
        {
            graphite.Loaded += graphite_Loaded;
        }

        void graphite_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSample();

        }

        

        private void Adder_OnClick(object sender, RoutedEventArgs e)
        {
            graphite.AddRandomNodes(1);

        }

        private void Deleter_OnClick(object sender, RoutedEventArgs e)
        {
            Node n = graphite.Nodes[0];
            graphite.DeleteNode(n);
        }
    }
    public class InfoBlob : DependencyObject
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Foto { get; set; }
        public int Born { get; set; }
        public int Died { get; set; }
    }

    #region WPF converters
    public class NameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InfoBlob)
            {
                var blob = value as InfoBlob;
                return string.Format("{0} {1}", blob.FirstName, blob.LastName);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PictureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InfoBlob)
            {

                var blob = value as InfoBlob;
                if (string.IsNullOrEmpty(blob.Foto)) blob.Foto = "unknown.gif";
                var imageSourceConverter = new ImageSourceConverter();
                return
                    imageSourceConverter.ConvertFromString(@"pack://application:,,,/WpfApplication1;Component/images/" +
                                                           blob.Foto) as ImageSource;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BornDiedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InfoBlob)
            {
                var blob = value as InfoBlob;
                string born = "°" + blob.Born;
                string died = blob.Died == 0 ? "" : " - †" + blob.Died;
                return born + died;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    public class GraphImporter
    {
        private static readonly Random rand = new Random();

        readonly Dictionary<string, Node> nodeDict = new Dictionary<string, Node>();
        private readonly GraphCanvas canvas;
        public void Load()
        {
            if (canvas != null)
            {
                canvas.NewDiagram(false);
                Import(GetType().Assembly.GetManifestResourceStream("WpfApplication1.SampleGraph.xml"));

            }
        }

        public GraphImporter(GraphCanvas canvas)
        {
            this.canvas = canvas;
        }

        /// <summary>
        /// Imports the specified stream.
        /// Note that we are very permissive for mistakes in the XML,
        /// here you can do as you please of course.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void Import(Stream stream)
        {
            try
            {
                string xmldoc;
                var reader = new StreamReader(stream);
                xmldoc = reader.ReadToEnd();
                XDocument doc = XDocument.Parse(xmldoc);
                if (doc == null)
                {
                    return;
                }
                Node n;
                #region Load the nodes
                foreach (XElement element in doc.Root.Elements())
                {
                    if (element.Attributes("id").Count() == 0)
                    {
                        continue; //ID is mendatory
                    }

                    if (element.Name == "Person")
                    {
                        n = new Node
                                {
                                    Type = NodeType.Person,
                                    Info = GetBlob(element),
                                    ID = element.Attribute("id").Value
                                };
                    }
                    else if (element.Name == "Idea")
                    {
                        n = new Node {Type = NodeType.Idea, ID = element.Attribute("id").Value};
                        if (element.Attributes("Title").Count() > 0)
                        {
                            n.Title = element.Attribute("Title").Value;
                        }
                        else
                        {
                            n.Title = "?";
                        }
                        if (element.Attributes("Info").Count() > 0)
                        {
                            n.Info = element.Attribute("Info").Value;
                        }
                        else
                        {
                            n.Info = "Not specified";
                        }
                    }
                    else if (element.Name == "Node")
                    {
                        n = new Node {Type = NodeType.Standard, ID = element.Attribute("id").Value};
                        if (element.Attributes("Title").Count() > 0)
                        {
                            n.Title = element.Attribute("Title").Value;
                        }
                        else
                        {
                            n.Title = "?";
                        }
                    }
                    else
                    {
                        continue;//unknown node type
                    }
                    if (n != null)
                    {
                        nodeDict.Add(n.ID, n);
                        canvas.AddNode(n);
                    }
                }
                #endregion
                #region Load the edges

                foreach (XElement element in doc.Root.Elements())
                {
                    try
                    {
                        if (element.Attributes("Links").Count() > 0)
                        {
                            string[] links = element.Attribute("Links").Value.Split(',');
                            string id = element.Attribute("id").Value;
                            foreach (string s in links)
                            {

                                try
                                {
                                    canvas.AddEdge(nodeDict[id], nodeDict[s],rand.Next(15,784).ToString() );
                                }
                                catch (Exception)
                                {
                                    continue;//impossible link or missing node
                                }
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                #endregion
            }
            catch (Exception)
            {
                //shouldn't happen
            }

        }
        private static InfoBlob GetBlob(XElement e)
        {
            var blob = new InfoBlob();

            if (e.Attributes("FirstName").Count() > 0)
            {
                blob.FirstName = e.Attribute("FirstName").Value;
            }
            else
            {
                blob.FirstName = "?";
            }
            if (e.Attributes("LastName").Count() > 0)
            {
                blob.LastName = e.Attribute("LastName").Value;
            }
            else
            {
                blob.LastName = "?";
            }
            if (e.Attributes("Foto").Count() > 0)
            {
                blob.Foto = e.Attribute("Foto").Value;
            }
            else
            {
                blob.Foto = "";
            }
            if (e.Attributes("Born").Count() > 0)
            {
                blob.Born = int.Parse(e.Attribute("Born").Value);
            }
            else
            {
                blob.Born =0;
            }
            if (e.Attributes("Died").Count() > 0)
            {
                blob.Died = int.Parse(e.Attribute("Died").Value);
            }
            else
            {
                blob.Died = 0;
            }
            return blob;
        }
    }
}
