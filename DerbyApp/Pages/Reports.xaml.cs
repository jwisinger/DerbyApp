#warning Y-REPORT: Could I somehow generate winners certificates along with "appearance" winners?

using DerbyApp.RacerDatabase;
using System.Windows;
using System.Windows.Controls;

namespace DerbyApp.Pages
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class Reports : Page
    {
        private readonly Database _db;
        private readonly bool _timeBasedScoring;
        private readonly string _eventName;
        private readonly string _outputFolderName;

        public Reports(Database db, bool timeBasedScoring, string eventName, string outputFolderName)
        {
            InitializeComponent();
            _db = db;
            _timeBasedScoring = timeBasedScoring;
            _eventName = eventName;
            _outputFolderName = outputFolderName;
            cbRace1.DataContext = _db.Races;
            cbRace2.DataContext = _db.Races;
            cbRace3.DataContext = _db.Races;

#warning Y-REPORT: Fix report
            /*foreach (string raceName in _db.Races)
            {
                _db.CurrentRaceName = raceName;
                if (_db.CurrentRaceRacers.Count == 0) continue;
                RaceResults results = new(raceName, _db.CurrentRaceRacers, RaceFormats.Formats[_db.RaceFormatIndex].Clone(), _db.GetHeatCount(_db.CurrentRaceName));
                int heatCount = _db.GetHeatCount(results.RaceName);
                while (results.RaceFormat.HeatCount < heatCount) results.AddRunOffHeat(null);
                _db.LoadResultsTable(results);
                _races.Add(results);
            }
            if (_races.Count > 0) cbRace1.SelectedItem = _races[0].RaceName;
            if (_races.Count > 1) cbRace2.SelectedItem = _races[1].RaceName;
            if (_races.Count > 2) cbRace3.SelectedItem = _races[2].RaceName;*/
        }

        private void ButtonReport_Click(object sender, RoutedEventArgs e)
        {
#warning Y-REPORT: Fix report
            //GenerateReport.Generate(_eventName, _db.GetName(), _outputFolderName, _db.Racers, _races, _timeBasedScoring);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
#warning Y-REPORT: Fix report
            /*
            if (_races.Count == 0) return;
            RaceResults result = _races.Where(x => x.RaceName == ((sender as ComboBox).SelectedItem as string)).First();
            if (result != null)
            {
                Leaderboard ldr = new(result.Racers, result.RaceFormat.HeatCount, result.RaceFormat.LaneCount, _timeBasedScoring);
                ldr.CalculateResults(result.ResultsTable);
                List<Racer> raceResults;
                if (_timeBasedScoring) raceResults = [.. ldr.Board.OrderBy(x => x.Score)];
                else raceResults = [.. ldr.Board.OrderByDescending(x => x.Score)];

                if (raceResults.Count > 0)
                {
                    switch ((sender as ComboBox).Name)
                    {
                        case "cbRace1":
                            if (raceResults.Count > 0) racer11Image.DataContext = raceResults[0]; else racer11Image.DataContext = null;
                            if (raceResults.Count > 0) racer11Name.DataContext = raceResults[0]; else racer11Name.DataContext = null;
                            if (raceResults.Count > 1) racer12Image.DataContext = raceResults[1]; else racer12Image.DataContext = null;
                            if (raceResults.Count > 1) racer12Name.DataContext = raceResults[1]; else racer12Name.DataContext = null;
                            if (raceResults.Count > 2) racer13Image.DataContext = raceResults[2]; else racer13Image.DataContext = null;
                            if (raceResults.Count > 2) racer13Name.DataContext = raceResults[2]; else racer13Name.DataContext = null;
                            racer11Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer11Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer12Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer12Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer13Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer13Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            break;
                        case "cbRace2":
                            if (raceResults.Count > 0) racer21Image.DataContext = raceResults[0]; else racer21Image.DataContext = null;
                            if (raceResults.Count > 0) racer21Name.DataContext = raceResults[0]; else racer21Name.DataContext = null;
                            if (raceResults.Count > 1) racer22Image.DataContext = raceResults[1]; else racer22Image.DataContext = null;
                            if (raceResults.Count > 1) racer22Name.DataContext = raceResults[1]; else racer22Name.DataContext = null;
                            if (raceResults.Count > 2) racer23Image.DataContext = raceResults[2]; else racer23Image.DataContext = null;
                            if (raceResults.Count > 2) racer23Name.DataContext = raceResults[2]; else racer23Name.DataContext = null;
                            racer21Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer21Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer22Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer22Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer23Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer23Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            break;
                        case "cbRace3":
                            if (raceResults.Count > 0) racer31Image.DataContext = raceResults[0]; else racer31Image.DataContext = null;
                            if (raceResults.Count > 0) racer31Name.DataContext = raceResults[0]; else racer31Name.DataContext = null;
                            if (raceResults.Count > 1) racer32Image.DataContext = raceResults[1]; else racer32Image.DataContext = null;
                            if (raceResults.Count > 1) racer32Name.DataContext = raceResults[1]; else racer32Name.DataContext = null;
                            if (raceResults.Count > 2) racer33Image.DataContext = raceResults[2]; else racer33Image.DataContext = null;
                            if (raceResults.Count > 2) racer33Name.DataContext = raceResults[2]; else racer33Name.DataContext = null;
                            racer31Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer31Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer32Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer32Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            racer33Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                            racer33Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
                            break;
                    }
                }
            }*/
        }

        private void SlideShow_Click(object sender, RoutedEventArgs e)
        {
#warning Y-REPORT: Add a slideshow
        }
    }
}
