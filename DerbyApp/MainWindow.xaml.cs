﻿using System.IO;
using System.Windows;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using System;
using System.Linq;
using DerbyApp.Pages;
using DerbyApp.Helpers;
using Microsoft.Win32;
using System.Windows.Input;
using DerbyApp.Assistant;
using ClippySharp;
using System.Net.Http;
using System.Threading.Tasks;
using System.Drawing.Printing;
using static System.Windows.Forms.LinkLabel;

namespace DerbyApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Database _db;
        private string _databaseName = "";
        private string _eventName = "";
        private string _outputFolderName = "";
        private string _playSoundsIcon = "/Images/Sound.png";
        private string _trackStatusIcon = "/Images/Disconnected.png";
        private string _timeBasedScoringIcon = "/Images/Timer.png";
        private string _cameraEnabledIcon = "/Images/CameraEnabled.png";
        private string _agentEnabledIcon = "/Images/DatabaseRoleError.png";
        private string _menuHideIcon = "/Images/TableFillLeft.png";
        private string _timeBasedScoringText = "Time Based Scoring";

        private int _selectedCamera = 0;
        private EditRace _editRace;
        private RacerTableView _racerTableView;
        private RaceTracker _raceTracker;
        private Reports _reports;
        private readonly NewRacer _newRacer;
        private int _maxRaceTime = 10;
        private bool _displayPhotosChecked = true;
        private bool _menuBarChecked = false;
        private bool _playSoundsChecked = true;
        private bool _flipCameraChecked = false;
        private bool _timeBasedScoring = false;
        private Visibility _collapsedVisibility = Visibility.Visible;
        private AgentInterface _agentInterface;
        private readonly Announcer _announcer = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public TrulyObservableCollection<MenuItemViewModel> CharacterMenuItems { get; set; }
        public TrulyObservableCollection<MenuItemViewModel> VoiceMenuItems { get; set; }

        public bool PlaySoundsChecked
        {
            get => _playSoundsChecked;
            set
            {
                _playSoundsChecked = value;
                _announcer.Muted = !value;
            }
        }
        public string TrackStatusIcon
        {
            get => _trackStatusIcon;
            set
            {
                _trackStatusIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackStatusIcon)));
            }
        }
        public string PlaySoundsIcon
        {
            get => _playSoundsIcon;
            set
            {
                _playSoundsIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaySoundsIcon)));
            }
        }
        public string CameraEnabledIcon
        {
            get => _cameraEnabledIcon;
            set
            {
                _cameraEnabledIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraEnabledIcon)));
            }
        }
        public string AgentEnabledIcon
        {
            get => _agentEnabledIcon;
            set
            {
                _agentEnabledIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AgentEnabledIcon)));
            }
        }
        public string MenuHideIcon
        {
            get => _menuHideIcon;
            set
            {
                _menuHideIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MenuHideIcon)));
            }
        }
        public string TimeBasedScoringText
        {
            get => _timeBasedScoringText;
            set
            {
                _timeBasedScoringText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeBasedScoringText)));
            }
        }
        public string TimeBasedScoringIcon
        {
            get => _timeBasedScoringIcon;
            set
            {
                _timeBasedScoringIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeBasedScoringIcon)));
            }
        }
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

        public bool MenuBarChecked
        {
            get => _menuBarChecked;
            set => _menuBarChecked = value;
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

        private void DisplayPhotos_Checked(object sender, RoutedEventArgs e)
        {
            DisplayPhotosChecked = !DisplayPhotosChecked;
            if (DisplayPhotosChecked)
            {
                _racerTableView.DisplayPhotos = Visibility.Visible;
                _editRace.DisplayPhotos = Visibility.Visible;
                _raceTracker.DisplayPhotos = Visibility.Visible;
                CameraEnabledIcon ="/Images/CameraEnabled.png";

            }
            else
            {
                _racerTableView.DisplayPhotos = Visibility.Collapsed;
                _editRace.DisplayPhotos = Visibility.Collapsed;
                _raceTracker.DisplayPhotos = Visibility.Collapsed;
                CameraEnabledIcon = "/Images/CameraDisabled.png";
            }
        }

        private void MenuVisible_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void PlaySounds_Checked(object sender, RoutedEventArgs e)
        {
            PlaySoundsChecked = !PlaySoundsChecked;
            if (PlaySoundsChecked)
            {
                PlaySoundsIcon = "/Images/Sound.png";
            }
            else
            {
                PlaySoundsIcon = "/Images/AudioMute.png";
            }
        }

        private void QRCodeClicked(object sender, RoutedEventArgs e)
        {
            string link = "";

            InputBox ib = new("Please enter the link for the QR Code:", link);

            if ((bool)ib.ShowDialog()) link = ib.Input;
            _newRacer.QrCodeLink = link;
            _racerTableView.QrCodeLink = link;
            Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring, _maxRaceTime, _newRacer.QrCodeLink, _newRacer.QrPrinterName, _newRacer.LicensePrinterName);
        }

        private void FlipCameraBox_Checked(object sender, RoutedEventArgs e)
        {
            FlipCameraChecked = !FlipCameraChecked;
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
            Database.GetDatabaseRegistry(out string databaseName, out string activeRace, out _outputFolderName, out _timeBasedScoring, out _maxRaceTime, out string qrCodeLink, out string qrPrinterName, out string licensePrinterName);
            if (!File.Exists(databaseName))
            {
                DatabaseCreator dbc = new();
                if (!(bool)dbc.ShowDialog()) this.Close();
                databaseName = dbc.DatabaseFile;
                _outputFolderName = Path.GetDirectoryName(databaseName);
            }
            InitializeComponent();
            this.Title = "Current Event = " + Path.GetFileNameWithoutExtension(databaseName);
            _db = new Database(databaseName);
            Database.StoreDatabaseRegistry(databaseName, activeRace, _outputFolderName, _timeBasedScoring, _maxRaceTime, qrCodeLink, qrPrinterName, licensePrinterName);
            _databaseName = databaseName;
            _db.LoadRaceSettings(out _eventName);
            _editRace = new EditRace(_db)
            {
                CurrentRaceName = activeRace
            };
            _racerTableView = new RacerTableView(_db, _outputFolderName)
            {
                QrCodeLink = qrCodeLink,
                QrPrinterName = qrPrinterName,
                LicensePrinterName = licensePrinterName
            };
            _newRacer = new NewRacer(_outputFolderName, _db.EventFile)
            {
                QrCodeLink = qrCodeLink,
                QrPrinterName = qrPrinterName,
                LicensePrinterName = licensePrinterName
            };

            _newRacer.RacerAdded += Racer_RacerAdded;
            _racerTableView.RacerRemoved += RacerTableView_RacerRemoved;
            _editRace.RaceChanged += EditRace_RaceChanged;
            _editRace.RaceChanging += EditRace_RaceChanging;

            mainFrame.Navigate(new Default());
            CreateMenu();
            _ = TrackStatusCheck();
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

            _raceTracker = new RaceTracker(results, _db, _databaseName, _outputFolderName, _announcer)
            {
                DisplayPhotos = DisplayPhotosChecked ? Visibility.Visible : Visibility.Collapsed
            };
            if (_raceTracker.Results.CurrentHeatNumber > 1) _editRace.buttonAddRacer.IsEnabled = false;
            else _editRace.buttonAddRacer.IsEnabled = true;
            _raceTracker.HeatChanged += RaceTracker_HeatChanged;
            if (!e) _raceTracker.Results.InProgress = false;
            _raceTracker.LdrBoard.TimeBasedScoring = _timeBasedScoring;
            _raceTracker.LdrBoard.CalculateResults(_raceTracker.Results.ResultsTable);
            _raceTracker.MaxRaceTime = _maxRaceTime;
            _raceTracker.Replay.SelectedCamera = _selectedCamera;
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
                Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring, _maxRaceTime, _newRacer.QrCodeLink, _newRacer.QrPrinterName, _newRacer.LicensePrinterName);
                _db.LoadRaceSettings(out _eventName);
                _editRace = new EditRace(_db);
                _racerTableView = new RacerTableView(_db, _outputFolderName)
                {
                    QrCodeLink = _newRacer.QrCodeLink,
                    QrPrinterName = _newRacer.QrPrinterName,
                    LicensePrinterName = _newRacer.LicensePrinterName
                };
                _newRacer.OutputFolderName = _outputFolderName;
                _newRacer.EventFile = _db.EventFile;
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
            Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring, _maxRaceTime, _newRacer.QrCodeLink, _newRacer.QrPrinterName, _newRacer.LicensePrinterName);
        }

        private void ButtonReport_Click(object sender, RoutedEventArgs e)
        {
            _reports = new Reports(_db, _timeBasedScoring, _eventName, _outputFolderName);
            mainFrame.Navigate(_reports);
            _agentInterface.ReportAction();
        }

        private async Task TrackStatusCheck()
        {
            try
            {
                
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(5);
                string response = await client.GetStringAsync(new Uri("http://192.168.0.1/ping"));
                TrackStatusIcon = "/Images/Connected.png";
                if (_raceTracker != null) _raceTracker.TrackConnected = true;
            }
            catch
            {
                TrackStatusIcon = "/Images/Disconnected.png";
                if (_raceTracker != null) _raceTracker.TrackConnected = false;
            }
            _ = Task.Delay(5000).ContinueWith(t => TrackStatusCheck());
        }

        private void ButtonMenuBar_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer t = new()
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            if (_menuBarChecked)
            {
                MenuHideIcon = "/Images/TableFillLeft.png";
                t.Tick += TimeTickExpand;
                CollapsedVisibility = Visibility.Visible;
                _menuBarChecked = false;
            }
            else
            {
                MenuHideIcon = "/Images/TableFillRight.png";
                t.Tick += TimeTickCollapse;
                _menuBarChecked = true;
            }
            t.Start();
        }

        private void MakeAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            string announcement = "";

            InputBox ib = new("Please enter an annoucement:", announcement);

            if ((bool)ib.ShowDialog()) announcement = ib.Input;
            _announcer.Speak(announcement);
        }

        void TimeTickCollapse(object sender, EventArgs e)
        {
            buttonColumn.Width = new GridLength(buttonColumn.Width.Value - 2);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("buttonColumn"));
            if (buttonColumn.Width.Value < 36)
            {
                (sender as DispatcherTimer).Stop();
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
                if(_raceTracker != null) _raceTracker.Replay.SelectedCamera = _selectedCamera;
            }
        }

        private void SetMaxRaceTime_Click(object sender, RoutedEventArgs e)
        {
            NumericInput input = new("Enter the max race time in seconds", _maxRaceTime);
            if ((bool)input.ShowDialog()) _maxRaceTime = input.Input;
            _raceTracker.MaxRaceTime = _maxRaceTime;
            Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring, _maxRaceTime, _newRacer.QrCodeLink, _newRacer.QrPrinterName, _newRacer.LicensePrinterName);
        }

        private void TimeBasedScoring_Click(object sender, RoutedEventArgs e)
        {
            _timeBasedScoring = !_timeBasedScoring;
            if (_timeBasedScoring)
            {
                TimeBasedScoringIcon = "/Images/Timer.png";
                TimeBasedScoringText = "Time Based Scoring";
            }
            else
            {
                TimeBasedScoringIcon = "/Images/OrderedList.png";
                TimeBasedScoringText = "Order Based Scoring";
            }
            _raceTracker.LdrBoard.TimeBasedScoring = _timeBasedScoring;
            _raceTracker.LdrBoard.CalculateResults(_raceTracker.Results.ResultsTable);
            Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring, _maxRaceTime, _newRacer.QrCodeLink, _newRacer.QrPrinterName, _newRacer.LicensePrinterName);
        }

        private void EnablePhotos_Click(object sender, RoutedEventArgs e)
        {
        }

        private void EnableSound_Click(object sender, RoutedEventArgs e)
        {
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
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
                _newRacer.OutputFolderName = _outputFolderName;
            }
        }

        private void CreateMenu()
        {
            CharacterMenuItems = [];
            VoiceMenuItems = [];

            MenuItemViewModel item = new() { Header = "none", ParentList = CharacterMenuItems };
            item.SelectionChanged += Item_AgentChanged;
            CharacterMenuItems.Add(item);

            foreach (string s in AgentInterface.GetAgentList())
            {
                item = new MenuItemViewModel { Header = s, ParentList = CharacterMenuItems };
                item.SelectionChanged += Item_AgentChanged;
                CharacterMenuItems.Add(item);
            }

            foreach (string s in _announcer.GetVoiceNames())
            {
                item = new MenuItemViewModel { Header = s, ParentList = VoiceMenuItems };
                item.SelectionChanged += Item_VoiceChanged;
                VoiceMenuItems.Add(item);
            }

            DataContext = this;
            _agentInterface = new AgentInterface(agentImage);
        }

        private void Item_AgentChanged(object sender, string e)
        {
            if (e != "none")
            {
                AgentEnabledIcon = "/Images/DatabaseRole.png";
                string[][] sArray = AgentEnvironment.GetAgents();
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
            else
            {
                AgentEnabledIcon = "/Images/DatabaseRoleError.png";
                agentImage.Visibility = Visibility.Collapsed;
            }
        }

        private void Item_VoiceChanged(object sender, string e)
        {
            _ = _announcer.SelectVoice(e);
        }


        private void Item_PrinterChanged(object sender, RoutedEventArgs e)
        {
            PrinterSelect ps = new();
            if ((bool)ps.ShowDialog())
            {
                _newRacer.QrPrinterName = ps.qrPrinterBox.Text;
                _newRacer.LicensePrinterName = ps.licensePrinterBox.Text;
                _racerTableView.QrPrinterName = ps.qrPrinterBox.Text;
                _racerTableView.LicensePrinterName = ps.licensePrinterBox.Text;
                Database.StoreDatabaseRegistry(_databaseName, _editRace.CurrentRaceName, _outputFolderName, _timeBasedScoring, _maxRaceTime, _newRacer.QrCodeLink, _newRacer.QrPrinterName, _newRacer.LicensePrinterName);
            }
        }

        private void AgentImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _agentInterface.ClickAgent();
        }

        private void MainWindowName_Closed(object sender, EventArgs e)
        {
            _raceTracker?.Shutdown();
            _newRacer?.ReleaseCamera();
        }
    }
}
