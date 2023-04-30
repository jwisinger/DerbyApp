using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;

#warning FEATURE: Add ability to generate per racer and overall reports
#warning PRETTY: Move enable photo button to main screen and remove from other screens

namespace DerbyApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Database _db;
        public string _currentRace = "";
        private readonly string _databaseName = "";
        private readonly ObservableCollection<string> _raceList = new ObservableCollection<string>();
        private readonly EditRace _editRace;

        public ObservableCollection<string> RaceList { get { return _raceList; } }

        public string CurrentRace
        {
            get
            {
                return _currentRace;
            }
            set
            {
                _currentRace = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentRace"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            Database.GetDatabaseRegistry(out string databaseName, out string activeRace);
            if (!File.Exists(databaseName))
            {
                DatabaseCreator dbc = new DatabaseCreator();
                if (System.Windows.Forms.DialogResult.OK != dbc.ShowDialog()) this.Close();
                databaseName = dbc.DatabaseFile;
            }
            InitializeComponent();
            this.Title = "Current Event = " + Path.GetFileNameWithoutExtension(databaseName);
            _db = new Database(databaseName);
            CurrentRace = activeRace;
            Database.StoreDatabaseRegistry(databaseName, CurrentRace);
            UpdateRaceList();
            _databaseName = databaseName;
            _editRace = new EditRace(_db);
        }

        private void ButtonChangeDatabase_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
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
                Database.StoreDatabaseRegistry(databaseName, CurrentRace);
            }
        }

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            NewRacer nr = new NewRacer();
            if (nr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _db.AddRacerToRacerTable(nr.Racer);
            }
        }

        private void ButtonViewRacerTable_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new RacerTableView(_db));
        }

        private void ButtonSelectRace_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(_editRace);
        }

        private void ButtonStartRace_Click(object sender, RoutedEventArgs e)
        {
            if (_editRace.Racers.Count > 0)
            {
                RaceResults Race = new RaceResults(_editRace.cbName.Text, _editRace.Racers, RaceHeats.ThirteenCarsFourLanes.HeatCount);
                mainFrame.Navigate(new RaceTracker(Race, RaceHeats.ThirteenCarsFourLanes));
            }
            Database.StoreDatabaseRegistry(_databaseName, CurrentRace);
        }

        private void UpdateRaceList()
        {
            List<string> rl = _db.GetListOfRaces();
            _raceList.Clear();
            foreach (string r in rl) _raceList.Add(r);
        }
    }
}
