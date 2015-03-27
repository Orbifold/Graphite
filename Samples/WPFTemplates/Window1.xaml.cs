using System.Windows;
using Orbifold.Graphite;

namespace WPFTemplates
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

        private void Window1_OnLoaded(object sender, RoutedEventArgs e)
        {
            diagram.NewDiagram(false);
            //note that the templates should be defined in the App.xaml file and that the NodeType defines the key of the template.
            var person = new Node { Title = "Hello Person!", Type = NodeType.Person };
            diagram.AddNode(person);

            var standard = new Node { Title = "Hello standard!", Type = NodeType.Standard };
            diagram.AddNode(standard);
            //you can also use the Info property to bind custom classes to the nodes.
            var idea = new Node { Type = NodeType.Idea, Info = new MyStuff { FirstName = "Francois", LastName = "Vanderseypen" } };
            diagram.AddNode(idea);

            diagram.AddEdge(standard, person);



        }
    }
    //sample custom class bound to a node, but you can define anything you like.
    //In particular, use the entities created from the Entity Framework or any data access layer to visualize data in the diagram.
    public class MyStuff
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
