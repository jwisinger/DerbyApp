using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.Runtime.CompilerServices;

#warning FEATURE: Add ability to generate per racer and overall reports
#warning TODO: See if I still need "RaceList"

namespace DerbyApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Database _db;
        public string _currentRace = "";
        private readonly string _databaseName = "";
        private readonly ObservableCollection<string> _raceList = new ObservableCollection<string>();
        private readonly EditRace _editRace;
        private readonly RacerTableView _racerTableView;
        private RaceTracker _raceTracker;
        private bool _displayPhotosChecked = false;

        public ObservableCollection<string> RaceList { get { return _raceList; } }
        public event PropertyChangedEventHandler PropertyChanged;

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

        public bool DisplayPhotosChecked
        {
            get => _displayPhotosChecked;
            set => _displayPhotosChecked = value;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DisplayPhotosChecked)
            {
                _racerTableView.DisplayPhotos = Visibility.Visible;
                _editRace.DisplayPhotos = Visibility.Visible;
                _raceTracker.DisplayPhotos = Visibility.Visible;
                _raceTracker.CheckBox_Checked();
            }
            else
            {
                _racerTableView.DisplayPhotos = Visibility.Collapsed;
                _editRace.DisplayPhotos = Visibility.Collapsed;
                _raceTracker.DisplayPhotos = Visibility.Collapsed;
            }
        }

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
            _racerTableView = new RacerTableView(_db);
#warning TODO: 13 Cars 4 Lanes
            _raceTracker = new RaceTracker(new RaceResults(), RaceHeats.ThirteenCarsFourLanes, _db);
            mainFrame.Navigate(new Default());
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
                _racerTableView.UpdateRacerList();
                _editRace.UpdateRacerList();
            }
        }

        private void ButtonViewRacerTable_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(_racerTableView);
        }

        private void ButtonSelectRace_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(_editRace);
        }

        private void ButtonStartRace_Click(object sender, RoutedEventArgs e)
        {
            if (_editRace.Racers.Count > 0)
            {
#warning TODO: 13 Cars 4 Lanes
                RaceResults Race = new RaceResults(_editRace.cbName.Text, _editRace.Racers, RaceHeats.ThirteenCarsFourLanes.HeatCount);
                _raceTracker = new RaceTracker(Race, RaceHeats.ThirteenCarsFourLanes, _db);
                mainFrame.Navigate(_raceTracker);
            }
            else
            {
                mainFrame.Navigate(new Default());
                MessageBox.Show("Your currently selected race " + CurrentRace + " has no racers in it.");
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
