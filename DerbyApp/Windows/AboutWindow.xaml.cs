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

            textBox.Inlines.Add("Emgu.CV 4.12.0.5764 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/Emgu.CV/4.12.0.5764/License")
            };
            hyperLink.Inlines.Add("GPL v3\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Emgu.CV.Bitmap 4.12.0.5764 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/Emgu.CV.Bitmap/4.12.0.5764/License")
            };
            hyperLink.Inlines.Add("GPL v3\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Emgu.CV.runtime.windows 4.12.0.5764 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/Emgu.CV.runtime.windows/4.12.0.5764/License")
            };
            hyperLink.Inlines.Add("GPL v3\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Emgu.CV.UI 4.12.0.5764 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/Emgu.CV.UI/4.12.0.5764/License")
            };
            hyperLink.Inlines.Add("GPL v3\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("KeePassLib.Standard 2.57.1 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/GPL-2.0-or-later")
            };
            hyperLink.Inlines.Add("GPL v2\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("ClippySharp 5-July-2020 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://github.com/netonjm/ClippySharp/blob/master/LICENSE")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("PiperSharp 12-September-2024 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://github.com/Lyx52/PiperSharp/blob/master/LICENSE")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Newtonsoft.Json v13.0.4 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("PDFsharp - MigraDoc - GDI v6.2.4 - ");
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

            textBox.Inlines.Add("System.Drawing.Common v10.0.2 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("System.Speech v10.0.2 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("System.Text.Json v10.0.2 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("SharpCompress v0.44.2 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("NAudio.Core v2.2.1 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("QRCoder v1.7.0 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Microsoft.Data.Sqlite v10.0.2 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/MIT")
            };
            hyperLink.Inlines.Add("MIT\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Google.Apis.Drive.v3 v1.73.0.3996 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/Apache-2.0")
            };
            hyperLink.Inlines.Add("Apache-2.0\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("Npgsql v10.0.1 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://licenses.nuget.org/PostgreSQL")
            };
            hyperLink.Inlines.Add("PostgreSQL\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("NAudio v2.2.1 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/NAudio/2.2.1/License")
            };
            hyperLink.Inlines.Add("Custom\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("NETStandard.Library v2.0.3 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://github.com/dotnet/standard/blob/master/LICENSE.TXT")
            };
            hyperLink.Inlines.Add("Custom\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("GemBox.Pdf v2026.1.100 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://www.nuget.org/packages/GemBox.Pdf/2026.1.100/License")
            };
            hyperLink.Inlines.Add("Custom\n");
            hyperLink.RequestNavigate += Hyperlink_RequestNavigate;
            textBox.Inlines.Add(hyperLink);

            textBox.Inlines.Add("hidlibrary v3.3.40 - ");
            hyperLink = new()
            {
                NavigateUri = new Uri("https://github.com/mikeobrien/HidLibrary/blob/master/LICENSE")
            };
            hyperLink.Inlines.Add("Custom\n");
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
