using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DerbyApp
{
    public partial class RaceTracker : Window, INotifyPropertyChanged
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
            Race.PropertyChanged += Race_PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Race_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentHeatCount":
                    gridRaceResults.Columns[1].CellStyle = new Style(typeof(DataGridCell));
                    gridRaceResults.Columns[1].CellStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.LightBlue)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("gridRaceResults"));
                    break;
            }
        }

        private void ButtonNextHeat_Click(object sender, RoutedEventArgs e)
        {
            Race.CurrentHeatCount++;
        }
    }
}
