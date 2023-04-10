using System.Windows;

#warning FEATURE: Get race status from registry at start up
#warning FEATURE: Add ability to generate per racer and overall reports

namespace DerbyApp
{
    public partial class MainWindow : Window
    {
        private readonly Database _db = new Database();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonAddRacer_Click(object sender, RoutedEventArgs e)
        {
            NewRacer nr = new NewRacer();
            if (nr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _db.AddRacer(nr.Racer);
            }
        }

        private void ButtonViewRacerTable_Click(object sender, RoutedEventArgs e)
        {
            new RacerTableView(_db).ShowDialog();
        }

        private void ButtonCreateRace_Click(object sender, RoutedEventArgs e)
        {
            NewRace nr = new NewRace(_db);
            if (nr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                new RaceTracker(nr.Race).Show();
                /*if (_db.CreateRaceTable(nr.Race))
                {
                }*/
            }
        }
    }
}
