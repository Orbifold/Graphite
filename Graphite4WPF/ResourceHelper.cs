using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Orbifold.Graphite
{
    public class ResourceHelper
    {

        public static string ExecutingAssemblyName
        {
            get
            {
                var name = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                return name.Substring(0, name.IndexOf(','));
            }
        }

        public static Stream GetStream(string relativeUri, string assemblyName)
        {
            if (relativeUri.StartsWith("/"))
                relativeUri = relativeUri.Remove(0, 1);
            if (Application.Current == null) return null;
            var res = Application.GetResourceStream(new Uri(assemblyName + ";component/" + relativeUri, UriKind.Relative)) ??
                      Application.GetResourceStream(new Uri(relativeUri, UriKind.Relative));
            if (res != null)
            {
                return res.Stream;
            }
            return null;
        }
        public static ImageSource GetImage(string imageName)
        {

            try
            {
                var imageSourceConverter = new ImageSourceConverter();
                var name = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                return imageSourceConverter.ConvertFromString(string.Format(@"pack://application:,,,/{0};Component/resources/images/{1}", name, imageName)) as ImageSource;
            }
            catch (Exception)
            {
                return null;
            }

            //BitmapImage image = new BitmapImage(new Uri(@"pack://application:,,,/Orbifold.G2.Core;Component/resources/images/" + name, UriKind.RelativeOrAbsolute));
            //BitmapFrame frame = BitmapFrame.Create(image);
            //return frame;
        }

        public static Stream GetStream(string relativeUri)
        {
            return GetStream(relativeUri, ExecutingAssemblyName);
        }

        public static BitmapImage GetBitmap(string relativeUri, string assemblyName)
        {
            var s = GetStream(relativeUri, assemblyName);
            if (s == null) return null;
            using (s)
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = s;
                bmp.EndInit();
                return bmp;
            }
        }

        public static BitmapImage GetBitmap(string relativeUri)
        {
            return GetBitmap(relativeUri, ExecutingAssemblyName);
        }

        public static string GetString(string relativeUri, string assemblyName)
        {
            var s = GetStream(relativeUri, assemblyName);
            if (s == null) return null;
            using (var reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetString(string relativeUri)
        {
            return GetString(relativeUri, ExecutingAssemblyName);
        }


        public static object GetXamlObject(string relativeUri, string assemblyName)
        {
            var str = GetString(relativeUri, assemblyName);
            if (str == null) return null;
            var obj = System.Windows.Markup.XamlReader.Parse(str);
            return obj;
        }

        public static object GetXamlObject(string relativeUri)
        {
            return GetXamlObject(relativeUri, ExecutingAssemblyName);
        }

    }
}