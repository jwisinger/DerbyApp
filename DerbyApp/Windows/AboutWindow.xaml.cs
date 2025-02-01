using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace DerbyApp.Windows
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Title = "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            textBox.Inlines.Add("The following open source libraries are used in this software:\n\n");

            textBox.Inlines.Add("DirectShowLib.Standard v2.1.0 - ");
            Hyperlink hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/DirectShowLib.Standard/2.1.0/License")
            };
            hyperLink.Inlines.Add("LGPL v2.1\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Emgu.CV v4.9.0.5494 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/Emgu.CV/4.9.0.5494/License")
            };
            hyperLink.Inlines.Add("GPL v3\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Emgu.CV.Bitmap v4.9.0.5494 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/Emgu.CV.Bitmap/4.9.0.5494/License")
            };
            hyperLink.Inlines.Add("GPL v3\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Emgu.CV.runtime.windows v4.9.0.5494 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/Emgu.CV.runtime.windows/4.9.0.5494/License")
            };
            hyperLink.Inlines.Add("GPL v3\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Emgu.CV.UI v4.9.0.5494 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/Emgu.CV.UI/4.9.0.5494/License")
            };
            hyperLink.Inlines.Add("GPL v3\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Newtonsoft.Json v13.0.3 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("PDFsharp - MigraDoc - GDI v6.1.1 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("System.Data.SqlClient v4.9.0 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("System.Drawing.Common v8.0.1 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("System.Speech v9.0.0 v8.0.1 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("System.Text.Json v9.0.0 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("NETStandard.Library v2.0.3 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://github.com/dotnet/standard/blob/master/LICENSE.TXT")
            };
            hyperLink.Inlines.Add("Public Domain\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("System.Data.SQLite v1.0.119.0 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.sqlite.org/copyright.html")
            };
            hyperLink.Inlines.Add("Public Domain\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("System.Data.SQLite.Core v1.0.119.0 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.sqlite.org/copyright.html")
            };
            hyperLink.Inlines.Add("Public Domain\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("All Images are from Flaticon - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://media.flaticon.com/license/license.pdf")
            };
            hyperLink.Inlines.Add("Attribution\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
