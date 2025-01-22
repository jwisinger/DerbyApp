#warning 1 BUG: Pick optimal for each character for each action
#warning 2 FUN: Add a "get attention" or idle thing if user waits too long
#warning 2 FUN: Add a gesture right for instant replay
#warning 2 FUN: Computer could announce racers via speech synthesis, maybe add an avatar
#warning 3 REPORT: Add an actual report page to give options for per racer, per race and maybe overall
#warning 4 FUN: Could I somehow generate winners certificates along with "appearance" winners?
#warning 5 HELP: Improve Help?
#warning 6 APPEARANCE: Change "start race" button to just "race"?

using System.IO;
using System.Windows;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using System;
using System.Linq;
using System.Collections.Generic;
using DerbyApp.Pages;
using System.Collections.ObjectModel;
using DerbyApp.Helpers;
using Microsoft.Win32;
using System.Windows.Input;
using DerbyApp.Assistant;
using ClippySharp.Core;

namespace DerbyApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Database _db;
        private string _databaseName = "";
        private string _eventName = "";
        private string _outputFolderName = "";
        private int _selectedCamera = 0;
        private EditRace _editRace;
        private RacerTableView _racerTableView;
        private RaceTracker _raceTracker;
        private readonly NewRacer _newRacer;
        private bool _displayPhotosChecked = true;
        private bool _flipCameraChecked = true;
        private bool _timeBasedScoring = false;
        private Visibility _collapsedVisibility = Visibility.Visible;
        private AgentInterface _agentInterface;

        public event PropertyChangedEventHandler PropertyChanged;

        public TrulyObservableCollection<MenuItemViewModel> CharacterMenuItems { get; set; }

        public bool DisplayPhotosChecked
        {
            get => _displayPhotosChecked;
            set => _displayPhotosChecked = value;
        }

        public bool FlipCameraChecked
        {
            get => _flipCameraChecked;
            set => _flipCameraChecked = value;
        }

        public Visibility CollapsedVisibility
        {
            get => _collapsedVisibility;
            set
            {
                _collapsedVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CollapsedVisibility)));
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DisplayPhotosChecked)
            {
                _racerTableView.DisplayPhotos = Visibility.Visible;
                _editRace.DisplayPhotos = Visibility.Visible;
                _raceTracker.DisplayPhotos = Visibility.Visible;
            }
            else
            {
                _racerTableView.DisplayPhotos = Visibility.Collapsed;
                _editRace.DisplayPhotos = Visibility.Collapsed;
                _raceTracker.DisplayPhotos = Visibility.Collapsed;
            }
        }

        private void FlipCameraBox_Checked(object sender, RoutedEventArgs e)
        {
            if (FlipCameraChecked)
            {
                _newRacer.FlipImage = true;
                _raceTracker.Replay.FlipImage = true;
            }
            else
            {
                _newRacer.FlipImage = false;
                _raceTracker.Replay.FlipImage = false;
            }
        }

        public MainWindow()
        {
            Database.GetDatabaseRegistry(out string databaseName, out string activeRace, out _outputFolderName, out _timeBasedScoring);
            if (!File.Exists(databaseName))
            {
                DatabaseCreator dbc = new();
                if (!(bool)dbc.ShowDialog()) this.Close();
                databaseName = dbc.DatabaseFile;
                _outputFolderName = Path.GetDirectoryName(databaseName);
            }
            InitializeComponent();
            MenuItemScoring.IsChecked = _timeBasedScoring;
            this.Title = "Current Event = " + Path.GetFileNameWithoutExtension(databaseName);
            _db = new Database(databaseName);
            Database.StoreDatabaseRegistry(databaseName, activeRace, _outputFolderName, _timeBasedScoring);
            _databaseName = databaseName;
            _db.LoadRaceSettings(out _eventName);
            _editRace = new EditRace(_db)
            {
                CurrentRaceName = activeRace
            };
            _racerTableView = new RacerTableView(_db);
            _newRacer = new NewRacer();

            _newRacer.RacerAdded += Racer_RacerAdded;
            _racerTableView.RacerRemoved += RacerTableView_RacerRemoved;
            _editRace.RaceChanged += EditRace_RaceChanged;
            _editRace.RaceChanging += EditRace_RaceChanging;

            mainFrame.Navigate(new Default());
            CreateMenu();
        }

        private void RaceTracker_HeatChanged(object sender, EventArgs e)
        {
            _editRace.buttonAddRacer.IsEnabled = false;
        }

        private void EditRace_RaceChanging(object sender, ResponseEventArgs e)
        {
            e.Continue = false;
            if (_raceTracker.Results.InProgress)
            {
                if (MessageBoxResult.OK == MessageBox.Show(
                    "Adding or removing a racer will reset the race in progress and erase all results.",
                    "Race Results Will Be Erased", MessageBoxButton.OKCancel, MessageBoxImage.Warning))
                {
                    e.Continue = true;
                }
            }
            else
            {
                e.Continue = true;
            }
        }

        private void EditRace_RaceChanged(object sender, bool e)
        {
            RaceResults results = new(_editRace.CurrentRaceName, _editRace.Racers, RaceFormats.Formats[_editRace.RaceFormatIndex].Clone());
            int heatCount = _db.GetHeatCount(results.RaceName);
            while (results.RaceFormat.HeatCount < heatCount) results.AddRunOffHeat(null);
            _db.LoadResultsTable(results);

            _raceTracker = new RaceTracker(results, _db, _databaseName, _outputFolderName)
            {
                DisplayPhotos = DisplayPhotosChecked ? Visibility.Visible : Visibility.Collapsed
            };
            if (_raceTracker.Results.CurrentHeatNumber > 1) _editRace.buttonAddRacer.IsEnabled = false;
            else _editRace.buttonAddRacer.IsEnabled = true;
            _raceTracker.HeatChanged += RaceTracker_HeatChanged;
            if (!e) _raceTracker.Results.InProgress = false;
            _raceTracker.LdrBoard.TimeBasedScoring = _timeBasedScoring;
            _raceTracker.LdrBoard.CalculateResults(_raceTracker.Results.ResultsTable);
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
            OpenFileDialog dialog = new()
            {
                CheckFileExists = false,
                DefaultExt = "sqlite",
                FileName = "MyEvent",
                Filter = "SQLite files | *.sqlite",
                Title = "Choose Event Database"
            };
            if ((bool)dialog.ShowDialog())
            {
                _databaseName = dialog.FileName;
                this.Title = "Current Event = " + Path.GetFileNameWithoutExtension(_databaseName);
                _db = new Database(_databaseName);
                _outputFolderName = Path.GetDirectoryName(_databaseName);
                Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring);
                _db.LoadRaceSettings(out _eventName);
                _editRace = new EditRace(_db);
                _racerTableView = new RacerTableView(_db);
                _racerTableView.RacerRemoved += RacerTableView_RacerRemoved;
                _editRace.RaceChanged += EditRace_RaceChanged;
                mainFrame.Navigate(new Default());
            }
        }

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(_newRacer);
            _agentInterface.AddRacerAction();
        }

        private void ButtonViewRacerTable_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(_racerTableView);
            _agentInterface.ViewRacerAction();
        }

        private void ButtonSelectRace_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(_editRace);
            _agentInterface.SelectRaceAction();
        }

        private void ButtonStartRace_Click(object sender, RoutedEventArgs e)
        {
            _agentInterface.StartRaceAction();
            if (_editRace.Racers.Count > 0)
            {
                mainFrame.Navigate(_raceTracker);
            }
            else
            {
                mainFrame.Navigate(new Default());
                MessageBox.Show("Your currently selected race " + _editRace.CurrentRaceName + " has no racers in it.");
            }
            Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring);
        }

        private void ButtonReport_Click(object sender, RoutedEventArgs e)
        {
            List<RaceResults> races = [];
            _agentInterface.ReportAction();
            foreach (string raceName in _db.GetListOfRaces())
            {
                (ObservableCollection<Racer> racers, int raceFormatIndex) = _db.GetRacers(raceName);
                RaceResults results = new(raceName, racers, RaceFormats.Formats[raceFormatIndex].Clone());
                int heatCount = _db.GetHeatCount(results.RaceName);
                while (results.RaceFormat.HeatCount < heatCount) results.AddRunOffHeat(null);
                _db.LoadResultsTable(results);
                races.Add(results);
            }
            GenerateReport.Generate(_eventName, _db.EventFile, _outputFolderName, _db.GetAllRacers(), races, _timeBasedScoring);
        }

        private void ButtonCollapse_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer t = new()
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            t.Tick += TimeTickCollapse;
            CollapseArrow.Visibility = Visibility.Hidden;
            t.Start();
        }

        private void ButtonExpand_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer t = new()
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

        private void HelpItem_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Help());
        }

        private void SetRaceName_Click(object sender, RoutedEventArgs e)
        {
            InputBox ib = new("Please enter a name for this event:", _eventName);

            if ((bool)ib.ShowDialog()) _eventName = ib.Input;
            _db.StoreRaceSettings(_eventName);
        }

        private void SelectCamera_Click(object sender, RoutedEventArgs e)
        {
            SelectCamera sc = new();
            if ((bool)sc.ShowDialog())
            {
                _selectedCamera = sc.GetSelectedCamera();
                _newRacer.SelectedCamera = _selectedCamera;
                _raceTracker.Replay.SelectedCamera = _selectedCamera;
            }
        }

        private void TimeBasedScoring_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.MenuItem).IsChecked) _timeBasedScoring = true;
            else _timeBasedScoring = false;
            _raceTracker.LdrBoard.TimeBasedScoring = _timeBasedScoring;
            _raceTracker.LdrBoard.CalculateResults(_raceTracker.Results.ResultsTable);
            Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring);
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("",
                "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(),
                MessageBoxButton.OK, MessageBoxImage.None);
        }

        private void OutDirItem_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                DefaultDirectory = Path.GetDirectoryName(_databaseName),
                Multiselect = false,
                Title = "Select Output Folder",
                ValidateNames = true
            }; 


            if (folderDialog.ShowDialog() == true)
            {
                _outputFolderName = folderDialog.FolderName;
                _raceTracker.OutputFolderName = _outputFolderName;
            }
        }

        private void CreateMenu()
        {
            CharacterMenuItems = [];

            MenuItemViewModel item = new MenuItemViewModel { Header = "none", ParentList = CharacterMenuItems };
            item.SelectionChanged += Item_AgentChanged;
            CharacterMenuItems.Add(item);

            foreach (string s in AgentInterface.GetAgentList())
            {
                item = new MenuItemViewModel { Header = s, ParentList = CharacterMenuItems };
                item.SelectionChanged += Item_AgentChanged;
                CharacterMenuItems.Add(item);
            }
            DataContext = this;
            _agentInterface = new AgentInterface(agentImage);
        }

        private void Item_AgentChanged(object sender, string e)
        {
            if (e != "none")
            {
                string[][] sArray = AgentEnvironment.Current.GetAgents();
                foreach (string[] s in sArray)
                {
                    if (s[1] == e)
                    {
                        agentImage.Visibility = Visibility.Visible;
                        _agentInterface.ChangeAgent(s[0]);
                        break;
                    }
                }
            }
            else agentImage.Visibility = Visibility.Collapsed;
        }

        private void agentImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _agentInterface.ClickAgent();
        }
    }
}
