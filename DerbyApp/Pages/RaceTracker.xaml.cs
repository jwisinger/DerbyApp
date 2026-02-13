#warning CLEANUP: Put breakpoints in every function in this file and confirm they work
#warning TODO: (SIMPLIFY) - This code could use a good cleanup too especially around LeaderBoard
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using DerbyApp.Assistant;
using DerbyApp.Helpers;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;

namespace DerbyApp
{
    public partial class RaceTracker : Page, INotifyPropertyChanged
    {
        #region Child Classes
        private readonly Database _db;
        private readonly Announcer _announcer;
        private readonly VideoHandler _videoHandler;
        private readonly TrackController _trackController;
        #endregion

        #region Private Properties
        private Visibility _recordingVisibility = Visibility.Collapsed;
        private Visibility _buttonVisibility = Visibility.Visible;
        private Visibility _cancelButtonVisibility = Visibility.Visible;
        private Visibility _displayPhotos = Visibility.Collapsed;
        private string _enableBoxButtonText = "Enable Manual Control";
        private string _currentHeatLabelString = "Current Heat (1)";
        private string _raceCountDownString = "";
        private bool _previousHeatEnabled = false;
        private bool _cancelReplayEnabled = false;
        private bool _nextHeatEnabled = true;
        private int _maxRaceTime = 10;
        #endregion

        public Leaderboard LdrBoard { get; set; }

        #region Public Properties
        public int MaxRaceTime
        {
            get => _maxRaceTime;
            set
            {
                _maxRaceTime = value;
                DatabaseRegistry.StoreDatabaseRegistry(null, null, null, null, value, null, null, null, null);
            }
        }

        public bool TrackConnected
        {
            get => _trackController.TrackConnected;
            set => _trackController.TrackConnected = value;
        }
        #endregion

        #region UI Properties
        public Visibility DisplayPhotos
        {
            get => _displayPhotos;
            set
            {
                _displayPhotos = value;
                NotifyPropertyChanged();
            }
        }

        public bool PreviousHeatEnabled
        {
            get => _previousHeatEnabled;
            set
            {
                _previousHeatEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public bool CancelReplayEnabled
        {
            get => _cancelReplayEnabled;
            set
            {
                _cancelReplayEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility RecordingVisibility
        {
            get => _recordingVisibility;
            set
            {
                _recordingVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public bool NextHeatEnabled
        {
            get => _nextHeatEnabled;
            set
            {
                _nextHeatEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility ButtonVisibility
        {
            get => _buttonVisibility;
            set
            {
                _buttonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility CancelButtonVisibility
        {
            get => _cancelButtonVisibility;
            set
            {
                _cancelButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public string EnableBoxButtonText
        {
            get => _enableBoxButtonText;
            set
            {
                _enableBoxButtonText = value;
                NotifyPropertyChanged();
            }
        }

        public string RaceCountDownString
        {
            get => _raceCountDownString;
            set
            {
                _raceCountDownString = value;
                NotifyPropertyChanged();
            }
        }

        public string CurrentHeatLabelString
        {
            get
            {
                return _currentHeatLabelString;
            }
            set
            {
                _currentHeatLabelString = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler HeatChanged;
        #endregion

        #region Event Handlers
        private void OnImageCaptured(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                frameVideo.Source = _videoHandler.CurrentImageSource;
            }));
        }

        private void RaceTrackerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(gridLeaderBoard.ItemsSource);
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription("Score", ListSortDirection.Descending));
            dataView.Refresh();
            gridRaceResults.Columns[0].IsReadOnly = true;
            gridRaceResults.Columns[1].IsReadOnly = true;
            UpdateHeatUI();
            _videoHandler.ImageCaptured += OnImageCaptured;
        }

        private void RaceTrackerWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _videoHandler.ImageCaptured -= OnImageCaptured;
        }

        private void TrackController_TrackStateUpdated(object sender, int raceCountdownTime)
        {
            if (_trackController.TrackStateNumber < _trackController.TrackStates.Length)
            {
                _announcer.StartRace(_trackController.TrackStateNumber);
                //_ = TrackMessage(_trackController.TrackStateString);
                if (_trackController.TrackStateNumber == _trackController.TrackStates.Length - 1)
                {
                    RecordingVisibility = Visibility.Visible;
                    _videoHandler.StartRecording(Path.Combine(_db.OutputFolderName, "videos"), _db.EventName, _db.CurrentHeatNumber);
                }
                RaceCountDownString = raceCountdownTime.ToString() + " seconds remaining.";
            }
            else
            {
                if (raceCountdownTime == 0)
                {
#warning CLEANUP: Move this UI stuff (and what's inside ButtonGetTime) into it's own method since it's used in multiple places
                    RaceCountDownString = "";
                    ButtonGetTimes_Click(sender, null);
                }
                else
                {
                    RaceCountDownString = raceCountdownTime.ToString() + " seconds remaining.";
                    CancelButtonVisibility = Visibility.Collapsed;
                }
            }

        }
        #endregion

        #region Public Methods
        public void SetTimeBasedScoring(bool enabled)
        {
            LdrBoard.TimeBasedScoring = enabled;
            LdrBoard.CalculateResults(_db.ResultsTable, _db.HeatCount);
        }

        public void Shutdown()
        {
            _ = _announcer.Voice.Cancel();
        }
        #endregion

        # region Private Methods
        private void SetActiveHeatColumn()
        {
            Style style = new(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.LightGreen)));
            gridRaceResults.Columns[_db.CurrentHeatNumber + 1].HeaderStyle = style;

            Style style2 = new(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style2.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style2.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
            gridRaceResults.Columns[_db.CurrentHeatNumber].HeaderStyle = style2;
            gridRaceResults.Columns[_db.CurrentHeatNumber + 2].HeaderStyle = style2;
            HeatChanged?.Invoke(this, null);
        }

        private void UpdateHeatUI()
        {
#warning CLEANUP: Turns this into a switch statement that handles various states of button enabling and strings
            if (true/*_manualControlEnabled*/)
            {
                PreviousHeatEnabled = false;
                NextHeatEnabled = false;
                EnableBoxButtonText = "Cancel";
                ButtonVisibility = Visibility.Collapsed;
            }
            else
            {
                if (_db.CurrentHeatNumber >= _db.RaceFormat.HeatCount) NextHeatEnabled = false;
                else NextHeatEnabled = true;
                if (_db.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
                else PreviousHeatEnabled = true;
                EnableBoxButtonText = "Enable Manual Control";
                ButtonVisibility = Visibility.Visible;
            }

            CurrentHeatLabelString = "Current Heat (" + _db.CurrentHeatNumber + ")";
            _db.RaceFormat.UpdateDisplayedHeat(_db.CurrentHeatNumber, _db.CurrentRaceRacers);
        }
        #endregion

        #region Button Handlers
        private void ButtonNextHeat_Click(object sender, RoutedEventArgs e)
        {
            _db.CurrentHeatNumber++;
            UpdateHeatUI();
            SetActiveHeatColumn();
            _announcer.SayNames(_db.RaceFormat.CurrentRacers);
        }

        private void ButtonPreviousHeat_Click(object sender, RoutedEventArgs e)
        {
            _db.CurrentHeatNumber--;
            UpdateHeatUI();
            SetActiveHeatColumn();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            ButtonVisibility = Visibility.Collapsed;
            PreviousHeatEnabled = false;
            NextHeatEnabled = false;
            _announcer.StartRace(_trackController.TrackStateNumber);
            _ = _trackController.StartHeat(MaxRaceTime);
        }

        private void ButtonGetTimes_Click(object sender, RoutedEventArgs e)
        {
            ButtonVisibility = Visibility.Collapsed;
            CancelButtonVisibility = Visibility.Collapsed;
            _ = GetTimes();
        }

        private void ButtonCancelReplay_Click(object sender, RoutedEventArgs e)
        {
            _videoHandler.CancelReplay();
        }

        private void ButtonAnnounceNames_Click(object sender, RoutedEventArgs e)
        {
            _announcer.SayNames(_db.RaceFormat.CurrentRacers);
        }

        private void ButtonSilenceAnnouncer_Click(object sender, RoutedEventArgs e)
        {
            _announcer.Silence();
        }

        private void ButtonEnable_Click(object sender, RoutedEventArgs e)
        {
            /*if (_manualControlEnabled)
            {
                _manualControlEnabled = false;
                UpdateHeatUI();
                _ = TrackMessage("cancel");
            }
            else
            {
                _manualControlEnabled = true;
                UpdateHeatUI();
                _trackStepCounter = 0;
                _raceCountDownTime = MaxRaceTime;
                _announcer.StartRace(_trackStepCounter);
                DispatcherTimer t = new() { Interval = TimeSpan.FromMilliseconds(250) };
                t.Tick += CheckSwitch;
                t.Start();
                _ = TrackMessage(_trackStrings[_trackStepCounter++]);
            }*/
        }

        private void ZoomPicture(object sender, RoutedEventArgs e)
        {
            new ImageDisplay((sender as Image).Source, ((sender as Image).DataContext as Racer)).ShowDialog();
        }
        #endregion






        private void Datagrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn col && e.PropertyType == typeof(double))
            {
                col.Binding = new Binding(e.PropertyName) { StringFormat = "N3" };
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ResultsColumnAdded(object sender, PropertyChangedEventArgs e)
        {
            gridRaceResults.Columns.Add(new DataGridTextColumn() { Header = e.PropertyName, Binding = new System.Windows.Data.Binding(e.PropertyName) { StringFormat = "N3" } });
        }

        public RaceTracker(Database db, int selectedCamera, bool timeBasedScoring, Announcer announcer, Credentials credentials, VideoHandler videoHandler)
        {
            InitializeComponent();
            _announcer = announcer;
            _db = db;
            _trackController = new TrackController();
            _trackController.TrackStateUpdated += TrackController_TrackStateUpdated;
            _db.RaceFormat.UpdateDisplayedHeat(_db.CurrentHeatNumber, db.CurrentRaceRacers);
            LdrBoard = new Leaderboard(db.CurrentRaceRacers, _db.RaceFormat.HeatCount, _db.RaceFormat.LaneCount, false);
            gridRaceResults.AutoGeneratingColumn += Datagrid_AutoGeneratingColumn;
            gridRaceResults.DataContext = _db.ResultsTable.DefaultView;
            gridLeaderBoard.DataContext = LdrBoard.Board;
            _db.RaceFormat.CurrentRacers.CollectionChanged += CurrentRacers_CollectionChanged;
            CurrentHeatLabel.DataContext = this;
            LdrBoard.TimeBasedScoring = timeBasedScoring;
            LdrBoard.CalculateResults(_db.ResultsTable, _db.HeatCount);
            _db.ColumnAdded += ResultsColumnAdded;

            _videoHandler = videoHandler;
            _videoHandler.ReplayEnded += ReplayEnded;
            _videoHandler.VideoUploaded += VideoUploaded;
        }

        private void CurrentRacers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_db.RaceFormat.CurrentRacers.Count == 0) return;
            racer1Image.DataContext = _db.RaceFormat.CurrentRacers[0];
            racer1Name.DataContext = _db.RaceFormat.CurrentRacers[0];
            racer1Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            racer1Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();

            if (_db.RaceFormat.CurrentRacers.Count == 1) return;
            racer2Image.DataContext = _db.RaceFormat.CurrentRacers[1];
            racer2Name.DataContext = _db.RaceFormat.CurrentRacers[1];
            racer2Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            racer2Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();

            if (_db.RaceFormat.CurrentRacers.Count == 2) return;
            racer3Image.DataContext = _db.RaceFormat.CurrentRacers[2];
            racer3Name.DataContext = _db.RaceFormat.CurrentRacers[2];
            racer3Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            racer3Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();

            if (_db.RaceFormat.CurrentRacers.Count == 3) return;
            racer4Image.DataContext = _db.RaceFormat.CurrentRacers[3];
            racer4Name.DataContext = _db.RaceFormat.CurrentRacers[3];
            racer4Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            racer4Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
        }

        public void ReplayEnded(object sender, EventArgs e)
        {
            ButtonVisibility = Visibility.Visible;
            CancelButtonVisibility = Visibility.Visible;
            //_manualControlEnabled = false;
            EnableBoxButtonText = "Enable Manual Control";
            CancelReplayEnabled = false;
        }

        public void VideoUploaded(object sender, VideoUploadedEventArgs e)
        {
            _db.AddVideoToTable(e);
        }


        private void GridRaceResults_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            LdrBoard.CalculateResults(_db.ResultsTable, _db.HeatCount);
            _db.UpdateResultsTable((e.EditingElement as TextBox).Text, e.Column.DisplayIndex, e.Row.GetIndex());
        }

        /*private async void CheckSwitch(object sender, EventArgs e)
        {
            if (_manualControlEnabled)
            {
                string response = await TrackMessage("switch");
                if (response != null)
                {
                    string[] responseArray = response.Replace("Switch ", "").Replace("\r\n", "").Split(",");
                    if (responseArray.Length == 3)
                    {
                        if ((_trackStepCounter == 1) && (responseArray[2] == "0"))
                        {
                            TimeTickRace(null, null);
                        }
                        else if ((_trackStepCounter == 2) && (responseArray[1] == "0"))
                        {
                            TimeTickRace(null, null);
                        }
                        else if ((_trackStepCounter == 3) && (responseArray[0] == "0"))
                        {
                            (sender as DispatcherTimer).Stop();
                            TimeTickRace(null, null);
                            _raceTimer.Start();
                        }
                    }
                }
            }
            else
            {
                (sender as DispatcherTimer).Stop();
            }
        }*/



        private void ButtonAddRunoff_Click(object sender, RoutedEventArgs e)
        {
            _db.AddRunOffHeat();
            LdrBoard.AddRunOffHeat(_db.RaceFormat.HeatCount);
        }


        private async Task GetTimes()
        {
            if (TrackConnected)
            {
                try
                {
                    using HttpClient client2 = new();
                    client2.Timeout = TimeSpan.FromSeconds(5);
                    string reponse = await client2.GetStringAsync(new Uri("http://192.168.0.1/read"));
                    if (reponse.Contains("Times"))
                    {
                        string[] times;
                        try
                        {
                            times = reponse.Split(' ')[1].Split(',');
                        }
                        catch
                        {
                            MessageBox.Show("Received a bad response from track.", "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (times.Length < 4)
                        {
                            MessageBox.Show("Received a bad response from track.", "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            if (!float.TryParse(times[i], out float result))
                            {
                                MessageBox.Show("Received a bad response from track.", "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            try
                            {
                                DataRow dr = _db.ResultsTable.Rows.Find(_db.RaceFormat.CurrentRacers[i].Number);
                                if (dr != null)
                                {
                                    if (result < 0.1) result = 10.0F;
                                    dr["Heat " + _db.CurrentHeatNumber] = result;
                                    LdrBoard.CalculateResults(_db.ResultsTable, _db.HeatCount);
                                    _db.UpdateResultsTable(null, 0, 0);
                                }
                            }
                            catch { }
                        }
                        if (_db.CurrentHeatNumber < _db.RaceFormat.HeatCount)
                        {
                            ButtonNextHeat_Click(null, null);
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show(e.Message, "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            try
            {
                _videoHandler.ShowReplay();
            }
            catch { }
            RecordingVisibility = Visibility.Collapsed;
            CancelReplayEnabled = true;
            RaceCountDownString = "";
            if (_db.CurrentHeatNumber >= _db.RaceFormat.HeatCount) NextHeatEnabled = false;
            else NextHeatEnabled = true;
            if (_db.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
            else PreviousHeatEnabled = true;
        }
    }
}
