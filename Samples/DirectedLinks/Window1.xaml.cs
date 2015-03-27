using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Orbifold.Graphite;

namespace DirectedLinks
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            diagram.NewDiagram(false);
            var home = new Node { Title = "Center" };
            var parent = new Node { Title = "Parent" };
            var to1 = new Node { Title = "To 1" };
            var to2 = new Node { Title = "To 2" };

            diagram.AddNode(home);
            diagram.AddNode(parent);
            diagram.AddNode(to1);
            diagram.AddNode(to2);
            diagram.AddEdge(home, to1);
            diagram.AddEdge(home, to2);
            diagram.AddEdge(parent, home);

            foreach (var edge in diagram.ChildrenLines(home))
            {
                edge.Label = "Child";
            }
           


        }


    }
    public class MyCanvas : GraphCanvas
    {
        public List<VisualEdge> ChildrenLines(Node node)
        {
            return NodeStates[node].ChildrenLines.Select(pair => pair.Value).ToList();
        }
    }

public sealed class TipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return "";
        var v = value as Tip;
        int propid;
        if (int.TryParse(parameter.ToString(), out propid))
        {
            switch (propid)
            {
                case 1:
                    return string.Format("From: {0}", v.From.Title);
                case 2:
                    return string.Format("To: {0}", v.To.Title);
            }
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
}
