#warning REPORT: Add an actual report page to give options for per racer, per race and maybe overall
#warning REPORT: Could I somehow generate winners certificates along with "appearance" winners?

using DerbyApp.RacerDatabase;
using DerbyApp.RaceStats;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DerbyApp.Pages
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class Reports : Page
    {
        private Database _db;

        public Reports(Database db)
        {
            InitializeComponent();
            _db = db;
        }

        private void ButtonReport_Click(object sender, RoutedEventArgs e)
        {
            List<RaceResults> races = [];
            foreach (string raceName in _db.GetListOfRaces())
            {
                (ObservableCollection<Racer> racers, int raceFormatIndex) = _db.GetRacers(raceName);
                RaceResults results = new(raceName, racers, RaceFormats.Formats[raceFormatIndex].Clone());
                int heatCount = _db.GetHeatCount(results.RaceName);
                while (results.RaceFormat.HeatCount < heatCount) results.AddRunOffHeat(null);
                _db.LoadResultsTable(results);
                races.Add(results);
            }
            //GenerateReport.Generate(_eventName, _db.EventFile, _outputFolderName, _db.GetAllRacers(), races, _timeBasedScoring);
        }
    }
}
