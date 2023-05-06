using Microsoft.Win32;
using System.IO;
using System.Windows;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Threading;
using System.Windows.Threading;
using System;

#warning REPORTS: Add ability to generate per racer and overall reports
#warning PRETTY: Pick a better icon
#warning TODO: Enable database switching correctly??

namespace DerbyApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Database _db;
        private readonly string _databaseName = "";
        private EditRace _editRace;
        private RacerTableView _racerTableView;
        private RaceTracker _raceTracker;
        private bool _displayPhotosChecked = false;
        private Visibility _collapsedVisibility = Visibility.Visible;

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
            _racerTableView = new RacerTableView(_db);
            _raceTracker = new RaceTracker(new RaceResults(), RaceHeats.Default, _db);
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
                this.Title = "Current Event = " + Path.GetFileNameWithoutExtension(databaseName);
                _db = new Database(databaseName);
                Database.StoreDatabaseRegistry(databaseName, _editRace.CurrentRace);
                _editRace = new EditRace(_db);
                _racerTableView = new RacerTableView(_db);
                _raceTracker = new RaceTracker(new RaceResults(), RaceHeats.Default, _db);
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
                RaceResults Race = new RaceResults(_editRace.cbName.Text, _editRace.Racers, RaceHeats.ThirteenCarsFourLanes.HeatCount);
                _raceTracker = new RaceTracker(Race, RaceHeats.ThirteenCarsFourLanes, _db);
                mainFrame.Navigate(_raceTracker);
            }
            else
            {
                mainFrame.Navigate(new Default());
                MessageBox.Show("Your currently selected race " + _editRace.CurrentRace + " has no racers in it.");
            }
            Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRace);
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
            if (buttonColumn.Width.Value < 30)
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
    }
}
