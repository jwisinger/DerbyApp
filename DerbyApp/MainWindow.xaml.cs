using Microsoft.Win32;
using System.IO;
using System.Windows;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.ComponentModel;
using System.Windows.Threading;
using System;
using System.Linq;
using System.Collections.Generic;
using DerbyApp.Pages;

namespace DerbyApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Database _db;
        private readonly string _databaseName = "";
        private EditRace _editRace;
        private RacerTableView _racerTableView;
        private RaceTracker _raceTracker;
        private readonly NewRacer _newRacer;
        private bool _displayPhotosChecked = false;
        private Visibility _collapsedVisibility = Visibility.Visible;
        private bool _raceModified = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool DisplayPhotosChecked
        {
            get => _displayPhotosChecked;
            set => _displayPhotosChecked = value;
        }

        public Visibility CollapsedVisibility
        {
            get => _collapsedVisibility;
            set
            {
                _collapsedVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CollapsedVisibility"));
            }
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
            Database.StoreDatabaseRegistry(databaseName, activeRace);
            _databaseName = databaseName;
            _editRace = new EditRace(_db)
            {
                CurrentRace = activeRace
            };
            _editRace.Racers.CollectionChanged += Racers_CollectionChanged;
            _racerTableView = new RacerTableView(_db);
            _raceTracker = new RaceTracker(new RaceResults(), RaceHeats.Default, _db);
            _newRacer = new NewRacer();

            _newRacer.RacerAdded += Racer_RacerAdded;
            _racerTableView.RacerRemoved += RacerTableView_RacerRemoved;

            RaceResults Race = new RaceResults(_editRace.CurrentRace, _editRace.Racers, RaceHeats.ThirteenCarsFourLanes);
            _db.LoadResultsTable(Race.ResultsTable, _editRace.CurrentRace);
            _raceTracker = new RaceTracker(Race, RaceHeats.ThirteenCarsFourLanes, _db);

            mainFrame.Navigate(new Default());
        }

        private void Racers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _raceModified = true;
        }

        private void RacerTableView_RacerRemoved(object sender, EventArgs e)
        {
            Racer r = (e as RacerEventArgs).racer;
            _editRace.AllRacers.Remove(_editRace.AllRacers.First(x => x.Number == r.Number));
            _editRace.AvailableRacers.Remove(_editRace.AvailableRacers.First(x => x.Number == r.Number));
        }

        private void Racer_RacerAdded(object sender, EventArgs e)
        {
            _db.AddRacerToRacerTable(_newRacer.Racer);
            _racerTableView.UpdateRacerList();
            _editRace.UpdateRacerList();
            _newRacer.ClearRacer();
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
                this.Title = "Current Event = " + Path.GetFileNameWithoutExtension(databaseName);
                _db = new Database(databaseName);
                Database.StoreDatabaseRegistry(databaseName, _editRace.CurrentRace);
                _editRace = new EditRace(_db);
                _racerTableView = new RacerTableView(_db);
                _raceTracker = new RaceTracker(new RaceResults(), RaceHeats.Default, _db);
                _raceModified = true;
                mainFrame.Navigate(new Default());
            }
        }

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(_newRacer);
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
                if (_raceModified)
                {
                    _raceModified = false;
                    RaceResults Race = new RaceResults(_editRace.cbName.Text, _editRace.Racers, RaceHeats.ThirteenCarsFourLanes);
                    _raceTracker = new RaceTracker(Race, RaceHeats.ThirteenCarsFourLanes, _db);
                }
                mainFrame.Navigate(_raceTracker);
            }
            else
            {
                mainFrame.Navigate(new Default());
                MessageBox.Show("Your currently selected race " + _editRace.CurrentRace + " has no racers in it.");
            }
            Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRace);
        }

        private void ButtonReport_Click(object sender, RoutedEventArgs e)
        {
            List<RaceResults> races = new List<RaceResults>();
            foreach (string raceName in _db.GetListOfRaces())
            {
                RaceResults race = new RaceResults(raceName, _db.GetRacers(raceName), RaceHeats.ThirteenCarsFourLanes);
                _db.LoadResultsTable(race.ResultsTable, race.RaceName);
                races.Add(race);
            }
            GenerateReport.Generate(Path.GetFileNameWithoutExtension(_db.EventName), _db.GetAllRacers(), races);
        }

        private void ButtonCollapse_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer t = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            t.Tick += TimeTickCollapse;
            CollapseArrow.Visibility = Visibility.Hidden;
            t.Start();
        }

        private void ButtonExpand_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer t = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            t.Tick += TimeTickExpand;
            CollapsedVisibility = Visibility.Visible;
            ExpandArrow.Visibility = Visibility.Hidden;
            t.Start();
        }

        void TimeTickCollapse(object sender, EventArgs e)
        {
            buttonColumn.Width = new GridLength(buttonColumn.Width.Value - 2);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("buttonColumn"));
            if (buttonColumn.Width.Value < 36)
            {
                (sender as DispatcherTimer).Stop();
                ExpandArrow.Visibility = Visibility.Visible;
                CollapsedVisibility = Visibility.Hidden;
            }
        }

        void TimeTickExpand(object sender, EventArgs e)
        {
            buttonColumn.Width = new GridLength(buttonColumn.Width.Value + 2);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("buttonColumn"));
            if (buttonColumn.Width.Value > 250)
            {
                (sender as DispatcherTimer).Stop();
                CollapseArrow.Visibility = Visibility.Visible;
            }
        }

        private void MainWindowName_Closing(object sender, CancelEventArgs e)
        {
            _newRacer.VideoThread?.Abort();
            _newRacer.LocalWebCam?.Stop();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Help());
        }
    }
}
