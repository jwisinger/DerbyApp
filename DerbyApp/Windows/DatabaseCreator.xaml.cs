using System;
using System.Windows;

namespace DerbyApp.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseCreator.xaml
    /// </summary>
    public partial class DatabaseCreator : Window
    {
        public string DatabaseFile = "";

        public DatabaseCreator()
        {
            InitializeComponent();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new()
            {
                DefaultExt = ".sqlite",
                Filter = "SQLite files | *.sqlite",
                AddExtension = true,
                CheckPathExists = true,
                FileName = "MyEvent",
                ValidateNames = true,
                CheckFileExists = false
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) DatabaseFile = dlg.FileName;
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
