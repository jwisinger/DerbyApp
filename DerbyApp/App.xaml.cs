using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace DerbyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppStartup(object sender, StartupEventArgs args)
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            using (StreamWriter sw = new("Error.Log"))
            {
                sw.WriteLine(e.Exception.Message);
            }

            // Prevent default unhandled exception processing
            e.Handled = true;
        }
    }
}
