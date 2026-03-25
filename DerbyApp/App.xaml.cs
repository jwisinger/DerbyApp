using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DerbyApp.Helpers;

namespace DerbyApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(OnButtonClick), handledEventsToo: true);
            EventManager.RegisterClassHandler(typeof(MenuItem), MenuItem.ClickEvent, new RoutedEventHandler(OnButtonClick), handledEventsToo: true);
            EventManager.RegisterClassHandler(typeof(Image), Image.MouseLeftButtonDownEvent, new RoutedEventHandler(OnButtonClick), handledEventsToo: true);
            EventManager.RegisterClassHandler(typeof(Label), Label.MouseLeftButtonDownEvent, new RoutedEventHandler(OnButtonClick), handledEventsToo: true);
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null) return null;
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            else return FindParent<T>(parentObject);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // This handler will be called for every button click in the application
            if (sender is Button clickedButton)
            {
                FrameworkElement page = FindParent<Page>(sender as Control);
                page ??= FindParent<Window>(sender as Control);
                ErrorLogger.LogEvent($"Button Clicked: [{page}] {clickedButton.Content}");
            }
            else if (sender is MenuItem clickedMenuItem)
            {
                FrameworkElement page = FindParent<Page>(sender as Control);
                page ??= FindParent<Window>(sender as Control);
                ErrorLogger.LogEvent($"Menu Item Clicked: [{page}] {clickedMenuItem.ToolTip}");
            }
            else if (sender is Image clickedImage)
            {
                FrameworkElement page = FindParent<Page>(sender as Control);
                page ??= FindParent<Window>(sender as Control);
                ErrorLogger.LogEvent($"Image Clicked: [{page}] {clickedImage.Source}");
            }
            else if (sender is Label clickedLabel)
            {
                FrameworkElement page = FindParent<Page>(sender as Control);
                page ??= FindParent<Window>(sender as Control);
                ErrorLogger.LogEvent($"Label Clicked: [{page}] {clickedLabel.Content}");
            }
        }

        void AppStartup(object sender, StartupEventArgs args)
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLogger.LogError("Unhandled Exception", e.Exception);
            // Prevent default unhandled exception processing
            e.Handled = true;
        }
    }
}
