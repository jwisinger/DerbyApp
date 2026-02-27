#warning BUG: announcer is hit or miss in manual mode
using DerbyApp.Assistant;
using DerbyApp.Helpers;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using DerbyApp.Windows;
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace DerbyApp
{
    public partial class RaceTracker : Page, INotifyPropertyChanged
    {
        #region Enums
        private enum TrackState
        {
            Idle,
            WaitingForManualStart,
            Countdown,
            RaceInProgress,
            GettingTimes,
            ShowingReplay
        }
        #endregion

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
        private TrackState _trackState = TrackState.Idle;
        private string _enableBoxButtonText = "Enable Manual Control";
        private string _currentHeatLabelString = "Current Heat (1)";
        private string _raceCountDownString = "";
        private bool _previousHeatEnabled = false;
        private bool _cancelReplayEnabled = false;
        private bool _nextHeatEnabled = true;
        #endregion

        #region Public Properties
        public int MaxRaceTime
        {
            get => _trackController.MaxRaceTime;
            set => _trackController.MaxRaceTime = value;
        }

        public bool TrackConnected
        {
            get => _trackController.TrackConnected;
            set
            {
                _trackController.TrackConnected = value;
                TrackStatusChanged?.Invoke(this, value);
            }
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

        public Visibility EnableBoxButtonVisibility
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
        public event EventHandler<bool> TrackStatusChanged;
        #endregion

        #region Event Handlers
        private void GetTimeHandler(object sender, float[] times)
        {
            if (times != null)
            {
                for (int i = 0; i < times.Length; i++)
                {
                    DataRow dr = _db.ResultsTable.Rows.Find(_db.RaceFormat.CurrentRacers[i].Number);
                    if (dr != null) dr["Heat " + _db.CurrentHeatNumber] = times[i];
                }
                _db.UpdateResultsTable(null, 0, 0);
            }
            if (_db.CurrentHeatNumber < _db.RaceFormat.HeatCount) ButtonNextHeat_Click(null, null);
            if (_videoHandler.ShowReplay()) _trackState = TrackState.ShowingReplay;
            else _trackState = TrackState.Idle;
            UpdateUI();
        }

        private void OnImageCaptured(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
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
            UpdateUI();
            SetActiveHeatColumn();
            _videoHandler.ImageCaptured += OnImageCaptured;
        }

        private void RaceTrackerWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _videoHandler.ImageCaptured -= OnImageCaptured;
        }

        private void TrackController_TrackStatusUpdated(object sender, bool e)
        {
            if (e) TrackConnected = true;
            else TrackConnected = false;
        }

        private void TrackController_TrackStateUpdated(object sender, int raceCountdownTime)
        {
            if (_trackController.TrackStateNumber < _trackController.TrackStates.Length)
            {
                _announcer.StartRace(_trackController.TrackStateNumber);
                if (_trackController.TrackStateNumber == _trackController.TrackStates.Length - 1)
                {
                    _trackState = TrackState.RaceInProgress;
                    _videoHandler.StartRecording(_db.VideoFolderName, _db.CurrentRaceName, _db.CurrentHeatNumber);
                }
                RaceCountDownString = raceCountdownTime.ToString() + " seconds remaining.";
            }
            else
            {
                if (raceCountdownTime == 0)
                {
                    _videoHandler.StopRecording();
                    RaceCountDownString = "";
                }
                else
                {
                    RaceCountDownString = raceCountdownTime.ToString() + " seconds remaining.";
                }
            }
            UpdateUI();
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
            _trackState = TrackState.Idle;
            _trackController.ManualControlEnabled = false;
            UpdateUI();
        }

        public void VideoUploaded(object sender, VideoUploadedEventArgs e)
        {
            _db.AddVideoToTable(e);
        }

        private void Datagrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
#warning X-RUNOFF: Do I need this?
            if (e.Column is DataGridTextColumn col && e.PropertyType == typeof(double))
            {
                col.Binding = new Binding(e.PropertyName) { StringFormat = "N3" };
            }
        }

        private void GridRaceResults_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _db.UpdateResultsTable((e.EditingElement as TextBox).Text, e.Column.DisplayIndex, e.Row.GetIndex());
        }

        private void ResultsColumnAdded(object sender, PropertyChangedEventArgs e)
        {
#warning X-RUNOFF: Do I need this?
            gridRaceResults.Columns.Add(new DataGridTextColumn() { Header = e.PropertyName, Binding = new Binding(e.PropertyName) { StringFormat = "N3" } });
        }

        #endregion

        #region Public Methods
        public void Shutdown()
        {
            _ = _announcer.Voice.Cancel();
        }
        #endregion

        # region Private Methods
        private void SetActiveHeatColumn()
        {
            CurrentHeatLabelString = "Current Heat (" + _db.CurrentHeatNumber + ")";
            _db.RaceFormat.UpdateDisplayedHeat(_db.CurrentHeatNumber, _db.CurrentRaceRacers);

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
            if (_db.CurrentHeatNumber > 1) gridRaceResults.Columns[_db.CurrentHeatNumber].HeaderStyle = style2;
            if (_db.CurrentHeatNumber + 2 < gridRaceResults.Columns.Count) gridRaceResults.Columns[_db.CurrentHeatNumber + 2].HeaderStyle = style2;
            HeatChanged?.Invoke(this, null);
        }

        private void UpdateUI()
        {
            PreviousHeatEnabled = false;
            NextHeatEnabled = false;
            EnableBoxButtonText = "Enable Manual Control";
            ButtonVisibility = Visibility.Collapsed;
            EnableBoxButtonVisibility = Visibility.Visible;
            CancelReplayEnabled = false;
            RecordingVisibility = Visibility.Collapsed;
            switch (_trackState)
            {
                case TrackState.Idle:
                    PreviousHeatEnabled = _db.CurrentHeatNumber > 1;
                    NextHeatEnabled = _db.CurrentHeatNumber < _db.RaceFormat.HeatCount;
                    ButtonVisibility = Visibility.Visible;
                    RaceCountDownString = "";
                    break;
                case TrackState.WaitingForManualStart:
                    EnableBoxButtonText = "Cancel";
                    break;
                case TrackState.Countdown:
                    EnableBoxButtonVisibility = Visibility.Hidden;
                    break;
                case TrackState.RaceInProgress:
                    RecordingVisibility = Visibility.Visible;
                    EnableBoxButtonVisibility = Visibility.Hidden;
                    break;
                case TrackState.GettingTimes:
                    RecordingVisibility = Visibility.Visible;
                    EnableBoxButtonVisibility = Visibility.Hidden;
                    break;
                case TrackState.ShowingReplay:
                    EnableBoxButtonVisibility = Visibility.Hidden;
#warning TODO: Maybe put a special replay happening icon
                    //RecordingVisibility = Visibility.Visible;
                    CancelReplayEnabled = true;
                    break;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Button Handlers
        private void ButtonAddRunoff_Click(object sender, RoutedEventArgs e)
        {
            _db.AddRunOffHeat();
        }

        private void ButtonNextHeat_Click(object sender, RoutedEventArgs e)
        {
            _db.CurrentHeatNumber++;
            UpdateUI();
            SetActiveHeatColumn();
            _announcer.SayNames(_db.RaceFormat.CurrentRacers);
        }

        private void ButtonPreviousHeat_Click(object sender, RoutedEventArgs e)
        {
            _db.CurrentHeatNumber--;
            UpdateUI();
            SetActiveHeatColumn();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            _trackState = TrackState.Countdown;
            _announcer.StartRace(_trackController.TrackStateNumber);
            _ = _trackController.StartHeat();
            UpdateUI();
        }

        private void ButtonGetTimes_Click(object sender, RoutedEventArgs e)
        {
            _trackState = TrackState.GettingTimes;
            _ = _trackController.GetTimes();
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
            if (_trackController.ManualControlEnabled)
            {
                _trackState = TrackState.Idle;
                _trackController.TrackStateNumber = 0;
                _trackController.ManualControlEnabled = false;
            }
            else
            {
                _trackState = TrackState.WaitingForManualStart;
                _trackController.ManualControlEnabled = true;
                _announcer.StartRace(0);
            }
            UpdateUI();
        }

        private void ZoomPicture(object sender, RoutedEventArgs e)
        {
            new ImageDisplay((sender as Image).Source, ((sender as Image).DataContext as Racer)).ShowDialog();
        }
        #endregion

        #region Constructor
        public RaceTracker(Database db, Announcer announcer, VideoHandler videoHandler)
        {
            InitializeComponent();
            _announcer = announcer;
            _db = db;
            _db.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(Database.CurrentRaceName))
                {
                    _db.RaceFormat.CurrentRacers.CollectionChanged += CurrentRacers_CollectionChanged;
#warning TODO: When changing race, reset to heat 1
                    //_db.CurrentHeatNumber = 1;
                    _trackState = TrackState.Idle;
                    UpdateUI();
                    //SetActiveHeatColumn();
                }
            };

            CurrentHeatLabel.DataContext = this;
            gridLeaderBoard.DataContext = _db.LdrBoard.Board;
            gridRaceResults.DataContext = _db.ResultsTable.DefaultView;
            gridRaceResults.AutoGeneratingColumn += Datagrid_AutoGeneratingColumn;

            _trackController = new TrackController();
            _trackController.TrackStateUpdated += TrackController_TrackStateUpdated;
            _trackController.TrackTimesUpdated += GetTimeHandler;
            _trackController.TrackStatusUpdated += TrackController_TrackStatusUpdated;

            _db.RaceFormat.UpdateDisplayedHeat(_db.CurrentHeatNumber, db.CurrentRaceRacers);
            _db.RaceFormat.CurrentRacers.CollectionChanged += CurrentRacers_CollectionChanged;
            _db.ColumnAdded += ResultsColumnAdded;

            _videoHandler = videoHandler;
            _videoHandler.ReplayEnded += ReplayEnded;
            _videoHandler.VideoUploaded += VideoUploaded;
        }
        #endregion
    }
}
