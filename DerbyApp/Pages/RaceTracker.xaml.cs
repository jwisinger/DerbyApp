#warning 1: Add runoff capability to break ties
#warning 2: When a report is generated, ties are "broken" and listed in the order shown on the leader board (incorrectly)
#warning 9: There was a crash on Stella's PC when manually editing race times and pressing "enter"
#warning FUN: When clicking "start heat", should the PC do the countdown (and remote control the lights) ... this would mean bypassing the embedded countdown?

using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System;
using System.ComponentModel;
using System.Data;
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
        private bool _previousHeatEnabled = false;
        private bool _nextHeatEnabled = true;
        private string _currentHeatLabelString = "Current Heat (1)";
        private readonly Database _db = null;
        private readonly DispatcherTimer _startTimer;
        private readonly DispatcherTimer _raceTimer;

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

        public RaceTracker(RaceResults race, RaceHeat heat, Database db)
        {
            InitializeComponent();
            Results = race;
            Heat = heat;
            _db = db;
            heat.UpdateHeat(Results.CurrentHeatNumber, race.Racers);
            LdrBoard = new Leaderboard(race.Racers, heat.HeatCount, heat.LaneCount);
            gridRaceResults.AutoGeneratingColumn += Datagrid_AutoGeneratingColumn;
            gridRaceResults.DataContext = Results.ResultsTable.DefaultView;
            gridLeaderBoard.DataContext = LdrBoard.Board;
            gridCurrentHeat.DataContext = Heat.CurrentRacers;
            CurrentHeatLabel.DataContext = this;
            _startTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _raceTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(12) };
            _startTimer.Tick += TimeTickStart;
            _raceTimer.Tick += TimeTickRace;
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
            gridRaceResults.Columns[Results.CurrentHeatNumber + 2].HeaderStyle = style2;
            HeatChanged?.Invoke(this, null);
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

            Style style = new(typeof(DataGridColumnHeader))
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
            _startTimer.Start();
            _ = StartHeat();
        }

        private void ButtonGetTimes_Click(object sender, RoutedEventArgs e)
        {
            _startTimer.Start();
            _ = GetTimes();
        }

        private async Task StartHeat()
        {
            try
            {
                using HttpClient client2 = new();
                string reponse = await client2.GetStringAsync(new Uri("http://192.168.0.1/start"));
                _startTimer.Stop();
                _raceTimer.Start();
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show(e.Message, "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task GetTimes()
        {
            try
            {
                using HttpClient client2 = new();
                string reponse = await client2.GetStringAsync(new Uri("http://192.168.0.1/read"));
                _startTimer.Stop();
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
                            DataRow dr = Results.ResultsTable.Rows.Find(Heat.CurrentRacers[i].Number);
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
                    if (Results.CurrentHeatNumber < Results.HeatCount)
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

        private void TimeTickStart(object sender, EventArgs e)
        {
            _startTimer.Stop();
            MessageBox.Show("Unable to communicate with track.", "Track Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void TimeTickRace(object sender, EventArgs e)
        {
            _raceTimer.Stop();
            ButtonGetTimes_Click(sender, null);
        }
    }
}
