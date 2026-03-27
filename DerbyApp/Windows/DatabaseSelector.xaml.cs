using System;
using System.Collections.ObjectModel;
using System.Windows;
using DerbyApp.Helpers;
using DerbyApp.RacerDatabase;

namespace DerbyApp.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseSelector.xaml
    /// </summary>
    public partial class DatabaseSelector : Window
    {
        private readonly DatabasePostgres _dbConnect;
        public string DatabaseFile = "";
        public ObservableCollection<string> EventList = [];
        public string DatabaseName
            = "";
        public bool Sqlite = false;

        public DatabaseSelector(Credentials credentials)
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            _dbConnect = new DatabasePostgres("", credentials);
            InitializeComponent();
            eventListbox.DataContext = EventList;
            serverTextBox.DataContext = _dbConnect;
            usernameTextBox.DataContext = _dbConnect;
            if (_dbConnect.InitGood) ButtonConnect_Click(null, null);
            System.Windows.Input.Mouse.OverrideCursor = null;
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new()
            {
                Title = "Choose Event Database",
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
            Sqlite = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonAddRemote_Click(object sender, RoutedEventArgs e)
        {
            InputBox ib = new("Please enter a name for the event:", "myEvent");

            if ((bool)ib.ShowDialog())
            {
                _dbConnect.AddNewDatabase(ib.Input);
                ButtonConnect_Click(null, null);
            }
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            EventList.Clear();
            foreach (string s in _dbConnect.GetEventList()) EventList.Add(s);
            System.Windows.Input.Mouse.OverrideCursor = null;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //_dbConnect.Password = ((PasswordBox)sender).Password;
        }

        private void EventListbox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            string selectedEvent = (string)eventListbox.SelectedItem;

            if (selectedEvent != null)
            {
                DialogResult = true;
                DatabaseName = selectedEvent;
                Sqlite = false;
                Close();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string selectedEvent = (string)eventListbox.SelectedItem;

            if (selectedEvent != null)
            {
                if (MessageBoxResult.OK ==
                    MessageBox.Show("Are you sure you want to delete [" + selectedEvent + "]", "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Warning))
                {
                    _dbConnect.DeleteDatabase(selectedEvent);
                    EventList.Clear();
                    foreach (string s in _dbConnect.GetEventList()) EventList.Add(s);
                }
            }
        }
    }
}
