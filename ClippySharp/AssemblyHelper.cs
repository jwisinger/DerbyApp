using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClippySharp
{
    public static class AssemblyHelper
    {
        public static string ReadResourceString(string agent, string resource)
        {
            string resourceName = $"agents.{agent}.{resource}";

            var assembly = typeof(AssemblyHelper).Assembly;
            var name = assembly.GetName().Name;
            var fullPath = string.Format("{0}.Resources.{1}", name, resourceName);
            using Stream? stream = assembly.GetManifestResourceStream(fullPath);
            if (stream == null) return "";
            using StreamReader reader = new(stream);
            string result = reader.ReadToEnd();
            return result;
        }

        public static BitmapImage? ReadResourceImage(string agent, string resource)
        {
            string resourceName = $"agents.{agent}.{resource}";

            var assembly = typeof(AssemblyHelper).Assembly;
            var name = assembly.GetName().Name;
            var fullPath = string.Format("{0}.Resources.{1}", name, resourceName);
            using Stream? stream = assembly.GetManifestResourceStream(fullPath);
            if (stream != null)
            {
                BitmapImage? image = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var imageSource = new BitmapImage();
                    imageSource.BeginInit();
                    imageSource.StreamSource = stream;
                    imageSource.EndInit();
                    image = imageSource;
                });
                return image;
            }
            else return null;
        }
    }
}
