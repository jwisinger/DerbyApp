using DerbyApp.Assistant;
using DerbyApp.Helpers;
using DerbyApp.Pages;
using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System;
using System.Collections.Generic;
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

namespace DerbyApp
{
    public partial class RaceTracker : Page, INotifyPropertyChanged
    {
        private Visibility _displayPhotos = Visibility.Collapsed;
        private Visibility _recordingVisibility = Visibility.Collapsed;
        private bool _previousHeatEnabled = false;
        private bool _cancelReplayEnabled = false;
        private bool _nextHeatEnabled = true;
        private Visibility _buttonVisibility = Visibility.Visible;
        private string _currentHeatLabelString = "Current Heat (1)";
        private readonly Database _db = null;
        private readonly DispatcherTimer _raceTimer;
        private string _raceCountDownString = "";
        private int _raceCountDownTime = 0;
        public int MaxRaceTime = 10;
        public bool TrackConnected = false;
        private readonly Replay replay;
        private readonly string _databaseName;
        private readonly Announcer _announcer;

        public string OutputFolderName;
        public RaceResults Results { get; set; }
        public Leaderboard LdrBoard { get; set; }

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
        public string RaceCountDownString
        {
            get
            {
                return _raceCountDownString;
            }
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

        internal Replay Replay => replay;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler HeatChanged;

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

        public RaceTracker(RaceResults results, Database db, string databaseName, string outputFolderName, Announcer announcer)
        {
            InitializeComponent();
            _announcer = announcer;
            Results = results;
            _db = db;
            Results.RaceFormat.UpdateDisplayedHeat(Results.CurrentHeatNumber, results.Racers);
            LdrBoard = new Leaderboard(results.Racers, results.RaceFormat.HeatCount, results.RaceFormat.LaneCount, false);
            gridRaceResults.AutoGeneratingColumn += Datagrid_AutoGeneratingColumn;
            gridRaceResults.DataContext = Results.ResultsTable.DefaultView;
            gridLeaderBoard.DataContext = LdrBoard.Board;
            Results.RaceFormat.CurrentRacers.CollectionChanged += CurrentRacers_CollectionChanged;
            CurrentHeatLabel.DataContext = this;
            _raceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _raceTimer.Tick += TimeTickRace;
            LdrBoard.CalculateResults(Results.ResultsTable);
            Results.ColumnAdded += ResultsColumnAdded;
            _databaseName = databaseName;
            OutputFolderName = outputFolderName;

            replay = new Replay(frameVideo);
            replay.ReplayEnded += ReplayEnded;
        }

        private void CurrentRacers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Results.RaceFormat.CurrentRacers.Count == 0) return;
            racer1Image.DataContext = Results.RaceFormat.CurrentRacers[0];
            racer1Name.DataContext = Results.RaceFormat.CurrentRacers[0];
            racer1Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            racer1Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();

            if (Results.RaceFormat.CurrentRacers.Count == 1) return;
            racer2Image.DataContext = Results.RaceFormat.CurrentRacers[1];
            racer2Name.DataContext = Results.RaceFormat.CurrentRacers[1];
            racer2Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            racer2Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();

            if (Results.RaceFormat.CurrentRacers.Count == 2) return;
            racer3Image.DataContext = Results.RaceFormat.CurrentRacers[2];
            racer3Name.DataContext = Results.RaceFormat.CurrentRacers[2];
            racer3Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            racer3Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();

            if (Results.RaceFormat.CurrentRacers.Count == 3) return;
            racer4Image.DataContext = Results.RaceFormat.CurrentRacers[3];
            racer4Name.DataContext = Results.RaceFormat.CurrentRacers[3];
            racer4Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            racer4Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
        }

        public void ReplayEnded(object sender, EventArgs e)
        {
            ButtonVisibility = Visibility.Visible;
            CancelReplayEnabled = false;
        }

        private void ButtonNextHeat_Click(object sender, RoutedEventArgs e)
        {
            Results.CurrentHeatNumber++;
            CurrentHeatLabelString = "Current Heat (" + Results.CurrentHeatNumber + ")";
            Results.RaceFormat.UpdateDisplayedHeat(Results.CurrentHeatNumber, Results.Racers);
            if (Results.CurrentHeatNumber >= Results.RaceFormat.HeatCount) NextHeatEnabled = false;
            else NextHeatEnabled = true;
            if (Results.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
            else PreviousHeatEnabled = true;

            Style style = new(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.LightGreen)));
            gridRaceResults.Columns[Results.CurrentHeatNumber + 1].HeaderStyle = style;

            Style style2 = new(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style2.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style2.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
            gridRaceResults.Columns[Results.CurrentHeatNumber].HeaderStyle = style2;
            HeatChanged?.Invoke(this, null);
            _announcer.SayNames(Results.RaceFormat.CurrentRacers);
        }

        private void ButtonPreviousHeat_Click(object sender, RoutedEventArgs e)
        {
            Results.CurrentHeatNumber--;
            CurrentHeatLabelString = "Current Heat (" + Results.CurrentHeatNumber + ")";
            Results.RaceFormat.UpdateDisplayedHeat(Results.CurrentHeatNumber, Results.Racers);
            if (Results.CurrentHeatNumber >= Results.RaceFormat.HeatCount) NextHeatEnabled = false;
            else NextHeatEnabled = true;
            if (Results.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
            else PreviousHeatEnabled = true;

            Style style = new(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, new SolidColorBrush(Colors.LightGreen)));
            gridRaceResults.Columns[Results.CurrentHeatNumber + 1].HeaderStyle = style;
            Style style2 = new(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style2.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, FontWeights.Bold));
            style2.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
            gridRaceResults.Columns[Results.CurrentHeatNumber + 2].HeaderStyle = style2;
            HeatChanged?.Invoke(this, null);
        }

        private void GridRaceResults_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Results.UpdateResults((e.EditingElement as System.Windows.Controls.TextBox).Text, e.Column.DisplayIndex, e.Row.GetIndex());
            LdrBoard.CalculateResults(Results.ResultsTable);
            _db.UpdateResultsTable(Results.RaceName, Results.ResultsTable.Rows[e.Row.GetIndex()]);
        }

        private void RaceTrackerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(gridLeaderBoard.ItemsSource);
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription("Score", ListSortDirection.Descending));
            dataView.Refresh();
            gridRaceResults.Columns[0].IsReadOnly = true;
            gridRaceResults.Columns[1].IsReadOnly = true;

            Style style = new(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style.Setters.Add(new Setter(System.Windows.Controls.Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(System.Windows.Controls.Control.BackgroundProperty, new SolidColorBrush(Colors.LightGreen)));
            gridRaceResults.Columns[Results.CurrentHeatNumber + 1].HeaderStyle = style;
        }

        public void CheckBox_Checked()
        {
            DisplayPhotos = Visibility.Visible;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            ButtonVisibility = Visibility.Collapsed;
            PreviousHeatEnabled = false;
            NextHeatEnabled = false;
            _announcer.StartRace(1);
            //RaceCountDownString = "On Your Marks!";
            _ = StartHeat();
        }

        private void ButtonGetTimes_Click(object sender, RoutedEventArgs e)
        {
            ButtonVisibility = Visibility.Collapsed;
            _ = GetTimes();
        }

        private void ButtonAddRunoff_Click(object sender, RoutedEventArgs e)
        {
            Results.AddRunOffHeat(null);
            LdrBoard.AddRunOffHeat(Results.RaceFormat.HeatCount);
            _db.AddRunOffHeat(Results.RaceName, Results.RaceFormat.HeatCount);
        }

        private void ButtonCancelReplay_Click(object sender, RoutedEventArgs e)
        {
            Replay.Cancel();
        }

        private async Task StartHeat()
        {
            if (TrackConnected)
            {
                try
                {
                    using HttpClient client = new();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    string reponse = await client.GetStringAsync(new Uri("http://192.168.0.1/start"));
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show(e.Message, "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            _raceCountDownTime = MaxRaceTime;
            _raceTimer.Start();
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
                                DataRow dr = Results.ResultsTable.Rows.Find(Results.RaceFormat.CurrentRacers[i].Number);
                                if (dr != null)
                                {
                                    if (result < 0.1) result = 10.0F;
                                    dr["Heat " + Results.CurrentHeatNumber] = result;
                                    LdrBoard.CalculateResults(Results.ResultsTable);
                                    _db.UpdateResultsTable(Results.RaceName, dr);
                                }
                            }
                            catch { }
                        }
                        if (Results.CurrentHeatNumber < Results.RaceFormat.HeatCount)
                        {
                            ButtonNextHeat_Click(null, null);
                        }
                        else
                        {
                            int i = 3; // Start looking for tie in 3rd place
                            while (i > 0)
                            {
                                List<Racer> tiedRacers = LdrBoard.CheckForTie(i);
                                if (tiedRacers.Count > 1)
                                {
                                    Results.AddRunOffHeat(tiedRacers);
                                    LdrBoard.AddRunOffHeat(Results.RaceFormat.HeatCount);
                                    _db.AddRunOffHeat(Results.RaceName, Results.RaceFormat.HeatCount);
                                    ButtonNextHeat_Click(null, null);
                                    break;
                                }
                                else
                                {
                                    i--;
                                }
                            }
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show(e.Message, "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            Replay.ShowReplay();
            RecordingVisibility = Visibility.Collapsed;
            CancelReplayEnabled = true;
            RaceCountDownString = "";
            if (Results.CurrentHeatNumber >= Results.RaceFormat.HeatCount) NextHeatEnabled = false;
            else NextHeatEnabled = true;
            if (Results.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
            else PreviousHeatEnabled = true;
        }

        private void TimeTickRace(object sender, EventArgs e)
        {
            if (_raceCountDownTime == MaxRaceTime)
            {
                _announcer.StartRace(2);
                //RaceCountDownString = "Get set!!";
            }
            else if (_raceCountDownTime == MaxRaceTime - 1)
            {
                _announcer.StartRace(3);
                //RaceCountDownString = "Go!!!";
                RecordingVisibility = Visibility.Visible;
                Replay.StartRecording(Path.Combine(OutputFolderName, Path.GetFileNameWithoutExtension(_databaseName), "videos"), Results.RaceName, Results.CurrentHeatNumber);
            }
            else if (_raceCountDownTime == 0)
            {
                RaceCountDownString = "";
                _raceTimer.Stop();
                ButtonGetTimes_Click(sender, null);
            }
            else
            {
                RaceCountDownString = _raceCountDownTime.ToString() + " seconds remaining.";
            }
            _raceCountDownTime--;
        }

        private void ZoomPicture(object sender, RoutedEventArgs e)
        {
            new ImageDisplay((sender as Image).Source).ShowDialog();
        }
    }
}
