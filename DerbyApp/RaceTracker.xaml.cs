using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DerbyApp
{
#warning FEATURE: Add "get data from track" button
#warning FEATURE: Somehow highlight current heat on datagrid

    public partial class RaceTracker : Window, INotifyPropertyChanged
    {
        private Visibility _displayPhotos = Visibility.Collapsed;
        private bool _displayPhotosChecked = false;
        private bool _previousHeatEnabled = false;
        private bool _nextHeatEnabled = true;

        public Race Race { get; set; }
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
        public bool DisplayPhotosChecked { get => _displayPhotosChecked; set => _displayPhotosChecked = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public RaceTracker(Race race)
        {
            InitializeComponent();
            Race = race;
            gridRaceResults.DataContext = Race.RaceResultsTable.DefaultView;
            gridLeaderBoard.DataContext = Race.Leaderboard;
            gridCurrentHeat.DataContext = Race.CurrentHeatRacers;
            CurrentHeatLabel.DataContext = Race;
        }

        private void ButtonNextHeat_Click(object sender, RoutedEventArgs e)
        {
            Race.CurrentHeatNumber++;
            if (Race.CurrentHeatNumber >= Race.HeatInfo.MaxHeats) NextHeatEnabled = false;
            else NextHeatEnabled = true;
            if (Race.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
            else PreviousHeatEnabled = true;
        }

        private void ButtonPreviousHeat_Click(object sender, RoutedEventArgs e)
        {
            Race.CurrentHeatNumber--;
            if (Race.CurrentHeatNumber >= Race.HeatInfo.MaxHeats) NextHeatEnabled = false;
            else NextHeatEnabled = true;
            if (Race.CurrentHeatNumber <= 1) PreviousHeatEnabled = false;
            else PreviousHeatEnabled = true;
        }

        private void GridRaceResults_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Race.UpdateResults((e.EditingElement as TextBox).Text, e.Column.DisplayIndex, e.Row.GetIndex());
        }

        private void GridRaceResults_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.Row.GetIndex() >= Race.RaceResultsTable.Rows.Count)
            {
                e.Cancel = true;
            }
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DisplayPhotosChecked) DisplayPhotos = Visibility.Visible;
            else DisplayPhotos = Visibility.Collapsed;
        }
    }
}
