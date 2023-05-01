using DerbyApp.RaceStats;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DerbyApp
{
#warning FEATURE: Add "get data from track" button
#warning FEATURE: Somehow highlight current heat on datagrid
#warning TODO: Fix scaling on RaceTracker
#warning TODO: Store current race in registry or database

    public partial class RaceTracker : Page, INotifyPropertyChanged
    {
        private Visibility _displayPhotos = Visibility.Collapsed;
        private bool _previousHeatEnabled = false;
        private bool _nextHeatEnabled = true;
        private string _currentHeatLabelString = "Current Heat (1)";

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

        public RaceTracker(RaceResults race, RaceHeat heat)
        {
            InitializeComponent();
            Results = race;
            Heat = heat;
            heat.UpdateHeat(Results.CurrentHeatNumber, race.Racers);
            LdrBoard = new Leaderboard(race.Racers, heat.HeatCount);
            gridRaceResults.DataContext = Results.ResultsTable.DefaultView;
            gridLeaderBoard.DataContext = LdrBoard.Board;
            gridCurrentHeat.DataContext = Heat.CurrentRacers;
            CurrentHeatLabel.DataContext = this;
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
        }

        private void GridRaceResults_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Results.UpdateResults((e.EditingElement as TextBox).Text, e.Column.DisplayIndex, e.Row.GetIndex());
            LdrBoard.CalculateResults(Results.ResultsTable);
#warning DATABASE: Store updated race timing info into database each run
        }

        private void RaceTrackerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(gridLeaderBoard.ItemsSource);
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription("Score", ListSortDirection.Descending));
            dataView.Refresh();
            gridRaceResults.Columns[0].IsReadOnly = true;
            gridRaceResults.Columns[1].IsReadOnly = true;
        }

        public void CheckBox_Checked()
        {
            DisplayPhotos = Visibility.Visible;
        }
    }
}
