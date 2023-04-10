using System.Windows;

namespace DerbyApp
{
    public partial class RaceTracker : Window
    {
        public LeaderBoard Ldrboard { get; set; }
        public Race Race { get; set; }

        public RaceTracker(Race race)
        {
            InitializeComponent();
            Race = race;
            Ldrboard = new LeaderBoard();
            gridRaceSchedule.DataContext = Race.RaceScheduleTable.DefaultView;
            gridLeaderBoard.DataContext = Ldrboard.Table.DefaultView;
        }
    }
}
