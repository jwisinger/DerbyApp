using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

#warning TODO: Make "get data from track" auto increment heat when successful

namespace DerbyApp
{
    public partial class RaceTracker : Page, INotifyPropertyChanged
    {
        private Visibility _displayPhotos = Visibility.Collapsed;
        private bool _previousHeatEnabled = false;
        private bool _nextHeatEnabled = true;
        private string _currentHeatLabelString = "Current Heat (1)";
        private readonly Database _db = null;
        private readonly DispatcherTimer _startTimer;

        public RaceResults Results { get; set; }
        public RaceHeat Heat { get; set; }
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
        public bool NextHeatEnabled
        {
            get => _nextHeatEnabled;
            set
            {
                _nextHeatEnabled = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RaceTracker(RaceResults race, RaceHeat heat, Database db)
        {
            InitializeComponent();
            Results = race;
            Heat = heat;
            _db = db;
            heat.UpdateHeat(Results.CurrentHeatNumber, race.Racers);
            LdrBoard = new Leaderboard(race.Racers, heat.HeatCount);
            gridRaceResults.DataContext = Results.ResultsTable.DefaultView;
            gridLeaderBoard.DataContext = LdrBoard.Board;
            gridCurrentHeat.DataContext = Heat.CurrentRacers;
            CurrentHeatLabel.DataContext = this;
            _startTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _startTimer.Tick += TimeTickStart;
            LdrBoard.CalculateResults(Results.ResultsTable);
        }

        private void ButtonNextHeat_Click(object sender, RoutedEventArgs e)
        {
            Results.CurrentHeatNumber++;
            CurrentHeatLabelString = "Current Heat (" + Results.CurrentHeatNumber + ")";
            Heat.UpdateHeat(Results.CurrentHeatNumber, Results.Racers);
            if (Results.CurrentHeatNumber >= Heat.HeatCount) NextHeatEnabled = false;
            else NextHeatEnabled = true;
            if (Results.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
            else PreviousHeatEnabled = true;

            Style style = new Style(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.LightGreen)));
            gridRaceResults.Columns[Results.CurrentHeatNumber + 1].HeaderStyle = style;
            Style style2 = new Style(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style2.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style2.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
            gridRaceResults.Columns[Results.CurrentHeatNumber].HeaderStyle = style2;
        }

        private void ButtonPreviousHeat_Click(object sender, RoutedEventArgs e)
        {
            Results.CurrentHeatNumber--;
            CurrentHeatLabelString = "Current Heat (" + Results.CurrentHeatNumber + ")";
            Heat.UpdateHeat(Results.CurrentHeatNumber, Results.Racers);
            if (Results.CurrentHeatNumber >= Heat.HeatCount) NextHeatEnabled = false;
            else NextHeatEnabled = true;
            if (Results.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
            else PreviousHeatEnabled = true;

            Style style = new Style(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.LightGreen)));
            gridRaceResults.Columns[Results.CurrentHeatNumber + 1].HeaderStyle = style;
            Style style2 = new Style(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style2.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style2.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
            gridRaceResults.Columns[Results.CurrentHeatNumber + 2].HeaderStyle = style2;
        }

        private void GridRaceResults_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Results.UpdateResults((e.EditingElement as TextBox).Text, e.Column.DisplayIndex, e.Row.GetIndex());
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

            Style style = new Style(typeof(DataGridColumnHeader))
            {
                BasedOn = TryFindResource("baseStyle") as Style
            };
            style.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.Bold));
            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.LightGreen)));
            gridRaceResults.Columns[Results.CurrentHeatNumber + 1].HeaderStyle = style;
        }

        public void CheckBox_Checked()
        {
            DisplayPhotos = Visibility.Visible;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadStringCompleted += Client_DownloadStringCompleted;
                client.DownloadStringAsync(new Uri("http://192.168.0.1/start"));
                _startTimer.Start();
            }
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            _startTimer.Stop();
            if(e.Error != null)
            {
                MessageBox.Show(e.Error.ToString(), "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if(e.Result.Contains("Times"))
            {
                string[] times = e.Result.Split(' ')[1].Split(',');
                if(times.Length < 4)
                {
                    MessageBox.Show("Received a bad response from track.", "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                for (int i = 0; i < 4; i++)
                {
                    if(!float.TryParse(times[i], out float result))
                    {
                        MessageBox.Show("Received a bad response from track.", "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    DataRow dr = Results.ResultsTable.Rows.Find(Heat.CurrentRacers[i].Number);
                    if (dr != null)
                    {
                        dr["Heat " + Results.CurrentHeatNumber] = result;
                        LdrBoard.CalculateResults(Results.ResultsTable);
                        _db.UpdateResultsTable(Results.RaceName, dr);
                    }
                }
            }
        }

        private void TimeTickStart(object sender, EventArgs e)
        {
            _startTimer.Stop();
            MessageBox.Show("Unable to communicate with track.", "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ButtonGetTimes_Click(object sender, RoutedEventArgs e)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadStringCompleted += Client_DownloadStringCompleted;
                client.DownloadStringAsync(new Uri("http://192.168.0.1/read"));
                _startTimer.Start();
            }
        }
    }
}
