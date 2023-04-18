using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DerbyApp
{
    public partial class RaceTracker : Window
    {
        public Race Race { get; set; }

        public RaceTracker(Race race)
        {
            InitializeComponent();
            Race = race;
            gridRaceResults.DataContext = Race.RaceResultsTable.DefaultView;
            gridLeaderBoard.DataContext = Race.Ldrboard.Table.DefaultView;
            gridCurrentHeat.DataContext = Race.CurrentHeat.Table.DefaultView;
            CurrentHeatLabel.DataContext = Race;
        }

        private void ButtonNextHeat_Click(object sender, RoutedEventArgs e)
        {
            Race.CurrentHeatCount++;
        }

        private void GridRaceResults_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
#warning VULNERABILITY: Block editing of name and number ... use e.Cancel
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
        }
    }
}
