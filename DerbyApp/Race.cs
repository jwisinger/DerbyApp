using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using DerbyApp.RacerDatabase;

#warning DATABASE: Store updated race timing info into database each run

namespace DerbyApp
{
    public class Race : INotifyPropertyChanged
    {
        public string RaceName;
        public List<Racer> Racers;
        public bool InProgress;
        private int _currentHeatNumber = 1;
        private string _currentHeatLabel = "Current Heat (1)";
        public DataTable RaceResultsTable;
        public DataTable RaceSummaryResultsTable;
        public ObservableCollection<Racer> CurrentHeatRacers;
        public TrulyObservableCollection<Racer> Leaderboard;

        public event PropertyChangedEventHandler PropertyChanged;

        public HeatDetails HeatInfo;

        public string CurrentHeatLabel
        {
            get
            {
                return _currentHeatLabel;
            }
            set
            {
                _currentHeatLabel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentHeatLabel"));
            }
        }

        public int CurrentHeatNumber
        {
            get
            {
                return _currentHeatNumber;
            }
            set
            {
                _currentHeatNumber = value;
                CurrentHeatRacers.Clear();
                ObservableCollection<Racer> racers = HeatInfo.GetHeat(_currentHeatNumber);
                foreach (Racer r in racers) CurrentHeatRacers.Add(r);
                CurrentHeatLabel = "Current Heat (" + value + ")";
            }
        }

        public Race():this("", new List<Racer>(), RaceResults.ThirteenCarsFourLanes) { }

        public Race(string raceName, List<Racer> racers, HeatList heatlist)
        {
            int racerNum = 0;

            RaceName = raceName;
            Racers = racers; 
            InProgress = false;

            HeatInfo = new HeatDetails(heatlist, racers);
            CurrentHeatRacers = HeatInfo.GetHeat(_currentHeatNumber);

            Leaderboard = new TrulyObservableCollection<Racer>();
            foreach (Racer r in racers) Leaderboard.Add(r);

            RaceResultsTable = new DataTable();
            RaceSummaryResultsTable = new DataTable();
            RaceResultsTable.Columns.Add("Number", Type.GetType("System.Int32"));
            RaceSummaryResultsTable.Columns.Add("Number", Type.GetType("System.Int32"));
            RaceResultsTable.Columns.Add("Name", Type.GetType("System.String"));
            RaceSummaryResultsTable.Columns.Add("Name", Type.GetType("System.String"));
            for (int i = 1; i <= heatlist.RacerCount; i++)
            {
                RaceResultsTable.Columns.Add("Heat " + i, Type.GetType("System.Double"));
                RaceSummaryResultsTable.Columns.Add("Heat " + i, Type.GetType("System.Int32"));
            }
            foreach (Racer r in racers)
            {
                DataRow row1 = RaceResultsTable.NewRow();
                DataRow row2 = RaceSummaryResultsTable.NewRow();
                row1["Number"] = r.Number;
                row2["Number"] = r.Number;
                row1["Name"] = r.RacerName;
                row2["Name"] = r.RacerName;
                RaceResultsTable.Rows.Add(row1);
                RaceSummaryResultsTable.Rows.Add(row2);
                r.RaceOrder = racerNum++;
            }
        }

        public void UpdateResults(string newString, int column, int row)
        {
            if (row >= RaceResultsTable.Rows.Count) return;

            if (!double.TryParse(newString, out var v)) return;

            RaceResultsTable.Rows[row][column] = newString;
            for (int i = 1; i <= RaceResultsTable.Columns.Count - 2; i++)
            {
                List<Tuple<double, string>> l = new List<Tuple<double, string>>();
                foreach(DataRow dataRow in RaceResultsTable.Rows)
                {
                    if (dataRow["Heat " + i] == DBNull.Value)
                    {
                        continue; 
                    }
                    try
                    {
                        l.Add(Tuple.Create((double)dataRow["Heat " + i], (string)dataRow["Name"]));
                    }
                    catch { }
                }
                l.Sort();

                foreach (DataRow dataRow in RaceSummaryResultsTable.Rows)
                {
                    int index = l.FindIndex(x => x.Item2 == (string)dataRow["Name"]);
                    if (index >= 0)
                    {
                        dataRow["Heat " + i] = 4 - index;
                    }
                }
            }

            foreach (DataRow dataRow in RaceSummaryResultsTable.Rows)
            {
                Racer r = Leaderboard.Where(x=>x.Number == (Int32)dataRow["Number"]).FirstOrDefault();
                if (r != null)
                {
                    int total = 0;
                    for (int i = 2; i < dataRow.ItemArray.Length; i++)
                    {
                        if (dataRow.ItemArray[i] != DBNull.Value) total += (int)dataRow.ItemArray[i];
                    }
                    r.Score = total;
                }
            }
        }
    }
}
