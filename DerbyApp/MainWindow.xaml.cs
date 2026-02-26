#warning TEST(0): Put breakpoints in every function in this file and confirm they work
#warning REPORTS: Look into Racer/Database/GenerateReport.cs
#warning REPORTS: Look into Pages/Reports
#warning FUTURE: Update software licenses
#warning TEST(1): What happens if I lose database connection when adding a racer
#warning TEST(1): Test network loss with auto-write from track
#warning TEST(2): Test complete run of race with Postgres
#warning TEST(2): Test adding racers with 2 computers (make sure refresh works)
#warning TEST(2): Test creating races with 2 computers (make sure refresh works)
#warning TEST(2): Test running race on one computer while someone is adding racers from another PC
#warning TEST(2): Check vercel usage when running full race with two computers
#warning TEST(2): Check reports
#warning TODO(2): Can I make reports go to Google Drive?
#warning TODO(2): Can I show a racers position in the app?
#warning TEST(3): Test complete run of race with Sqlite
#warning TEST(4): What happens when I click the 2 refresh buttons with sqlite?
#warning TEST(4): What happens with funny characters in database or table names?
#warning FUTURE: Add lots of logging to the catch statements
#warning FUTURE: Allow changing picture (build this into the ImageDisplay?
#warning FUTURE: Move videos from retool to Gdrive?
#warning FUTURE: Add ability to copy local database to remote, mainly need a way to get a name for the remote database and then create it
using ClippySharp;
using DerbyApp.Assistant;
using DerbyApp.Helpers;
using DerbyApp.Pages;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DerbyApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Private Properties
        private bool _playSoundsChecked = true;
        private bool _displayPhotosChecked = true;
        private bool _flipCameraChecked = false;
        private bool _menuBarChecked = false;
        private string _trackStatusIcon = "/Images/Disconnected.png";
        private string _databaseStatusIcon = "/Images/DatabaseStop.png";
        private string _playSoundsIcon = "/Images/Sound.png";
        private string _cameraEnabledIcon = "/Images/CameraEnabled.png";
        private string _agentEnabledIcon = "/Images/DatabaseRoleError.png";
        private string _menuHideIcon = "/Images/TableFillLeft.png";
        private string _timeBasedScoringIcon = "/Images/Timer.png";
        private string _timeBasedScoringText = "Time Based Scoring";
        private string _copyDatabaseText = "Copy Database to Local";
        private Visibility _collapsedVisibility = Visibility.Visible;
        #endregion

        #region Pages
        private readonly Default _default = new();
        private readonly Help _help = new();
        private EditRace _editRace;
        private NewRacer _newRacer;
        private RaceTracker _raceTracker;
        private RacerTableView _racerTableView;
        private readonly Reports _reports;
        #endregion

        #region Child Classes
        private Database _db;
        private AgentInterface _agentInterface;
        private Credentials _credentials;
        private GoogleDriveAccess _googleDriveAccess;
        private VideoHandler _videoHandler;
        private readonly Announcer _announcer = new();
        #endregion

        #region Menu Items
        public TrulyObservableCollection<MenuItemViewModel> CharacterMenuItems { get; set; }
        public TrulyObservableCollection<MenuItemViewModel> VoiceMenuItems { get; set; }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public Properties
        public bool PlaySoundsChecked
        {
            get => _playSoundsChecked;
            set
            {
                _playSoundsChecked = value;
                _announcer.Muted = !value;
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

        public string TrackStatusIcon
        {
            get => _trackStatusIcon;
            set
            {
                _trackStatusIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackStatusIcon)));
            }
        }

        public string DatabaseStatusIcon
        {
            get => _databaseStatusIcon;
            set
            {
                _databaseStatusIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatabaseStatusIcon)));
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

        public string TimeBasedScoringIcon
        {
            get => _timeBasedScoringIcon;
            set
            {
                _timeBasedScoringIcon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeBasedScoringIcon)));
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

        public string CopyDatabaseText
        {
            get => _copyDatabaseText;
            set
            {
                _copyDatabaseText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CopyDatabaseText)));
            }
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
        #endregion

        #region Navigation Handlers
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
            if (_db.CurrentRaceRacers.Count > 0)
            {
                mainFrame.Navigate(_raceTracker);
            }
            else
            {
                mainFrame.Navigate(_default);
                MessageBox.Show("Your currently selected race " + _db.CurrentRaceName + " has no racers in it.");
            }
            _agentInterface.StartRaceAction();
        }

        private void ButtonReport_Click(object sender, RoutedEventArgs e)
        {
#warning REPORTS: Fix this
            //_reports = new Reports(_db);
            //mainFrame.Navigate(_reports);
            _agentInterface.ReportAction();
        }

        private void HelpItem_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(_help);
        }
        #endregion

        #region Menu Bar Handlers
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
        #endregion

        #region Checkbox Handlers
        private void DisplayPhotos_Checked(object sender, RoutedEventArgs e)
        {
            DisplayPhotosChecked = !DisplayPhotosChecked;
            if (DisplayPhotosChecked)
            {
                _racerTableView.DisplayPhotos = Visibility.Visible;
                _editRace.DisplayPhotos = Visibility.Visible;
                _raceTracker.DisplayPhotos = Visibility.Visible;
                CameraEnabledIcon = "/Images/CameraEnabled.png";

            }
            else
            {
                _racerTableView.DisplayPhotos = Visibility.Collapsed;
                _editRace.DisplayPhotos = Visibility.Collapsed;
                _raceTracker.DisplayPhotos = Visibility.Collapsed;
                CameraEnabledIcon = "/Images/CameraDisabled.png";
            }
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

        private void FlipCameraBox_Checked(object sender, RoutedEventArgs e)
        {
            FlipCameraChecked = !FlipCameraChecked;
            if (FlipCameraChecked) _videoHandler.FlipImage = true;
            else _videoHandler.FlipImage = false;
        }
        #endregion

        #region Menu Item Handlers
        private void Item_PrinterChanged(object sender, RoutedEventArgs e)
        {
            PrinterSelect ps = new();
            if ((bool)ps.ShowDialog())
            {
                _db.QrPrinter = ps.qrPrinterBox.Text;
                _db.LicensePrinter = ps.licensePrinterBox.Text;
            }
        }

        private void Item_AgentChanged(object sender, string e)
        {
            bool found = false;
            if (e != "none")
            {
                string[] matchingAgent = AgentEnvironment.GetAgents().FirstOrDefault(s => s[1] == e);
                if (matchingAgent != null)
                {
                    found = true;
                    AgentEnabledIcon = "/Images/DatabaseRole.png";
                    agentImage.Visibility = Visibility.Visible;
                    _agentInterface.ChangeAgent(matchingAgent[0]);
                }
            }
            if (!found)
            {
                AgentEnabledIcon = "/Images/DatabaseRoleError.png";
                agentImage.Visibility = Visibility.Collapsed;
            }
        }

        private void Item_VoiceChanged(object sender, string e)
        {
            _ = _announcer.SelectVoice(e);
        }

        private void SetRaceName_Click(object sender, RoutedEventArgs e)
        {
            InputBox ib = new("Please enter a name for this event:", _db.EventName);

            if ((bool)ib.ShowDialog()) _db.EventName = ib.Input;
        }

        private void CopyDatabaseToLocal_Click(object sender, RoutedEventArgs e)
        {
            _db.CopyDatabaseToLocal();
        }

        private void ButtonChangeDatabase_Click(object sender, RoutedEventArgs e)
        {
            SelectDatabase();
        }

        private void MakeAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            string announcement = "";

            InputBox ib = new("Please enter an annoucement:", announcement);

            if ((bool)ib.ShowDialog()) announcement = ib.Input;
            _announcer.Speak(announcement);
        }

        private void UpdateTimeBasedScoringInfo()
        {
            if (_db.TimeBasedScoring)
            {
                TimeBasedScoringIcon = "/Images/OrderedList.png";
                TimeBasedScoringText = "Order Based Scoring";
            }
            else
            {
                TimeBasedScoringIcon = "/Images/Timer.png";
                TimeBasedScoringText = "Time Based Scoring";
            }
        }

        private void TimeBasedScoring_Click(object sender, RoutedEventArgs e)
        {
            _db.TimeBasedScoring = !_db.TimeBasedScoring;
            UpdateTimeBasedScoringInfo();
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void AgentImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _agentInterface.ClickAgent();
        }

        private void SelectCamera_Click(object sender, RoutedEventArgs e)
        {
            SelectCamera sc = new();
            if ((bool)sc.ShowDialog())
            {
                _videoHandler.SelectedCamera = sc.GetSelectedCamera();
            }
        }

        private void SetMaxRaceTime_Click(object sender, RoutedEventArgs e)
        {
            NumericInput input = new("Enter the max race time in seconds", _raceTracker.MaxRaceTime);
            if ((bool)input.ShowDialog()) _raceTracker.MaxRaceTime = input.Input;
        }

        private void OutDirItem_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                DefaultDirectory = Path.GetDirectoryName(_db.OutputFolderName),
                Multiselect = false,
                Title = "Select Output Folder",
                ValidateNames = true
            };

            if (folderDialog.ShowDialog() == true)
            {
                _db.OutputFolderName = folderDialog.FolderName;
            }
        }

        private void QRCodeClicked(object sender, RoutedEventArgs e)
        {
            InputBox ib = new("Please enter the link for the QR Code:", _db.QrCodeLink);
            if ((bool)ib.ShowDialog()) _db.QrCodeLink = ib.Input;
        }
        #endregion

        #region Child Event Handlers
        private void Racer_RacerAdded(object sender, EventArgs e)
        {
            _db.AddRacer(new Racer(_newRacer.Racer));
            _newRacer.ClearRacer();
        }

        private void RaceTracker_HeatChanged(object sender, EventArgs e)
        {
            _editRace.buttonAddRacer.IsEnabled = false;
        }

        private void Database_SyncChanged(object sender, EventArgs e)
        {
            if (_db.IsSynced) DatabaseStatusIcon = "/Images/DatabaseRun.png";
            else DatabaseStatusIcon = "/Images/DatabaseStop.png";
        }
        #endregion

        #region General Methods
        private bool SelectDatabase()
        {
            bool retVal = false;
            DatabaseSelector dbs = new(_credentials);
            while ((bool)dbs.ShowDialog())
            {
                string databaseName;
                if (dbs.Sqlite) databaseName = dbs.DatabaseFile;
                else databaseName = dbs.EventName;
                DatabaseRegistry.StoreDatabaseRegistry(databaseName, null, null, null, null, null, null, null, null);
                if (ChangeDatabase()) break;
            }
            if (retVal) CopyDatabaseText = "Upload Database to Remote";
            else CopyDatabaseText = "Copy Database to Local";

            return retVal;
        }

        private bool ChangeDatabase()
        {
            DatabaseRegistry.GetDatabaseRegistry(out string databaseName, out string activeRace, out string outputFolderName, out bool timeBasedScoring, out int maxRaceTime, out string qrCodeLink, out string qrPrinterName, out string licensePrinterName, out string password);
            _credentials = new Credentials(password);
            DatabaseRegistry.StoreDatabaseRegistry(null, null, null, null, null, null, null, null, _credentials.Password);
            _googleDriveAccess = new GoogleDriveAccess(_credentials);
            _videoHandler = new(_credentials);
            _db = new Database(databaseName, _credentials, _googleDriveAccess, outputFolderName, timeBasedScoring);
            UpdateTimeBasedScoringInfo();

            if (_db.InitGood)
            {
                Title = "Current Event = " + Path.GetFileNameWithoutExtension(databaseName);
                if (activeRace != null) _db.CurrentRaceName = activeRace;
                if (_db.IsSqlite) CopyDatabaseText = "Upload Database to Remote";
                else CopyDatabaseText = "Copy Database to Local";
                _db.SyncStatusChanged += Database_SyncChanged;

                _racerTableView = new RacerTableView(_db)
                {
                    DisplayPhotos = DisplayPhotosChecked ? Visibility.Visible : Visibility.Collapsed
                };

                _newRacer = new NewRacer(_db, _videoHandler);
                _newRacer.RacerAdded += Racer_RacerAdded;

                _raceTracker = new RaceTracker(_db, _announcer, _videoHandler)
                {
                    MaxRaceTime = maxRaceTime,
                    DisplayPhotos = DisplayPhotosChecked ? Visibility.Visible : Visibility.Collapsed
                };
                _raceTracker.HeatChanged += RaceTracker_HeatChanged;
                _raceTracker.TrackStatusChanged += (s, e) =>
                {
                    if (e) TrackStatusIcon = "/Images/Connected.png";
                    else TrackStatusIcon = "/Images/Disconnected.png";
                };

                _editRace = new EditRace(_db)
                {
                    DisplayPhotos = DisplayPhotosChecked ? Visibility.Visible : Visibility.Collapsed
                };

                mainFrame.Navigate(_default);
                return true;
            }
            else return false;
        }
        #endregion

        #region Main
        private void MainWindowName_Closed(object sender, EventArgs e)
        {
            _db.Close();
            _raceTracker?.Shutdown();
            _videoHandler?.ReleaseCamera();
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

        public MainWindow()
        {
            InitializeComponent();
            CreateMenu();
            if (!ChangeDatabase()) SelectDatabase();
        }
        #endregion
    }
}
