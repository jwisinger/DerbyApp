using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DerbyApp
{
#warning FEATURE: Add previous heat, or "jump to" heat button
#warning FEATURE: Add "get data from track" button
#warning FEATURE: Somehoe highlight current heat on datagrid

    public partial class RaceTracker : Window, INotifyPropertyChanged
    {
#warning CODE CLEANUP: "It would be nice to figure out how to make this a bool"
        private Visibility _displayPhotos = Visibility.Collapsed;
        private bool _displayPhotosChecked = false;

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
            gridLeaderBoard.DataContext = Race.Ldrboard.Racers;
            gridCurrentHeat.DataContext = Race.CurrentHeatRacers;
            CurrentHeatLabel.DataContext = Race;
        }

        private void ButtonNextHeat_Click(object sender, RoutedEventArgs e)
        {
#warning FEATURE: Add "end of race" after last heat
            Race.CurrentHeatCount++;
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
