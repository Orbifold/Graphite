using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Orbifold.Graphite
{
    /// <summary>
    /// Utility class.
    /// </summary>
    public static class Utils
    {
        private static readonly Random Rnd = new Random();
        private static readonly Stream SampleNamesStream = typeof(Utils).Assembly.GetManifestResourceStream("Orbifold.Graphite.physics.txt");
        private static readonly string[] Names = ReadAllLines(SampleNamesStream);
        public static NodeContent RandomTitle()
        {
            var fetch = Names[Rnd.Next(0, Names.Length)];
            var title = string.Empty;
            var info = string.Empty;
            if (fetch.Contains("/"))
            {
                title = fetch.Substring(0, fetch.IndexOf("/"));
                info = fetch.Substring(fetch.IndexOf("/") + 1);
            }
            else
            {
                title = fetch;
            }

            return new NodeContent { Info = info,Title = title};

        }
        public static string[] ReadAllLines(Stream s)
        {
            var reader = new StreamReader(s);
            var allLines = new List<string>();

            while (!reader.EndOfStream)
            {
                allLines.Add(reader.ReadLine().Trim());
            }
            return allLines.ToArray();
        }

        /// <summary>
        /// Returns the children nodes, i.e. the the target nodes of an arrow/edge.
        /// </summary>
        /// <param name="from">From.</param>
        /// <returns></returns>
        public static List<Node> ChildrenNodes(this Node from)
        {
            return from.Graph.ChildrenOf(from);
        }
        /// <summary>
        /// Returns the parent nodes, i.e. the the source nodes of an arrow/edge
        /// </summary>
        /// <param name="to">To.</param>
        /// <returns></returns>
        public static List<Node> ParentNodes(this Node to)
        {
            return to.Graph.ParentsOf(to);
        }
        /// <summary>
        /// Returns true if there is either a from/to or a to/from edge between the current node and the given node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="otherNode">The other node.</param>
       
        public static bool HasEdgeWith(this Node node, Node otherNode)
        {
            return node.Graph.HasEdge(node, otherNode);
        }
    }
    public struct NodeContent
    {
        public string Info;
        public string Title;
    }
}