using Microsoft.Win32;
using System.IO;
using System.Windows;

#warning FEATURE: Add ability to generate per racer and overall reports

namespace DerbyApp
{
    public partial class MainWindow : Window
    {
        private Database _db;

        public MainWindow()
        {
            Database.GetDatabaseRegistry(out string databaseName);
            if (!File.Exists(databaseName))
            {
                DatabaseCreator dbc = new DatabaseCreator();
                if (System.Windows.Forms.DialogResult.OK != dbc.ShowDialog()) this.Close();
                databaseName = dbc.DatabaseFile;
            }
            InitializeComponent();
            this.Title = Path.GetFileNameWithoutExtension(databaseName);
            _db = new Database(databaseName);
            Database.StoreDatabaseRegistry(databaseName); 
        }

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            NewRacer nr = new NewRacer();
            if (nr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _db.AddRacer(nr.Racer);
            }
        }

        private void ButtonViewRacerTable_Click(object sender, RoutedEventArgs e)
        {
            new RacerTableView(_db).ShowDialog();
        }

        private void ButtonCreateRace_Click(object sender, RoutedEventArgs e)
        {
            NewRace nr = new NewRace(_db);
            if (nr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                new RaceTracker(nr.Race).Show();
                /*if (_db.CreateRaceTable(nr.Race))
                {
                }*/
            }
        }

        private void ButtonChangeDatabase_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog() {
                CheckFileExists = false,
                DefaultExt = "sqlite",
                FileName = "MyEvent",
                Filter = "SQLite files | *.sqlite",
                Title = "Choose Event Database"
            };

            if ((bool)dialog.ShowDialog())
            {
                string databaseName = dialog.FileName;
                this.Title = Path.GetFileNameWithoutExtension(databaseName);
                _db = new Database(databaseName);
                Database.StoreDatabaseRegistry(databaseName);
            }
        }
    }
}
