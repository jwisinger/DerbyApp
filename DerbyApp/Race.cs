using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

#warning FEATURE: Add race schedule into the table (include N/A) if racer not involved in heat
#warning FEATURE: Add ability to run heats and update leader board (and totals) ... store into database each run
#warning FEATURE: Allow override of auto-recorded times just in case
#warning FEATURE: Auto pull times from the M5Stack

namespace DerbyApp
{
    public class Race : INotifyPropertyChanged
    {
        public string RaceName;
        public List<Racer> Racers;
        public bool InProgress;
        private int _currentHeatCount = 1;
        private string _currentHeatLabel = "Current Heat (1)";
        public DataTable RaceResultsTable;
        public DataTable RaceSummaryResultsTable;

        public event PropertyChangedEventHandler PropertyChanged;

        public LeaderBoard Ldrboard { get; set; }
        public Heat CurrentHeat { get; set; }
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

        public int CurrentHeatCount
        {
            get
            {
                return _currentHeatCount;
            }
            set
            {
                _currentHeatCount = value;
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
            Ldrboard = new LeaderBoard(racers);
            CurrentHeat = new Heat();
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
                racerNum++;
                /*for (int i = 0; i < heatlist.RacerCount; i++)
                {
                    int val = Array.IndexOf(heatlist.Heats[i], racerNum);
                    if (val >= 0) row["Heat " + (i + 1)] = val + 1;
                    else row["Heat " + (i + 1)] = " ";
                }*/
            }
        }

        public void UpdateResults(string newString, int column, int row)
        {
            if (row >= RaceResultsTable.Rows.Count) return;

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
                DataRow r = Ldrboard.Table.Rows.Find((Int32)dataRow["Number"]);
                if (r != null)
                {
                    int total = 0;
                    for (int i = 2; i < dataRow.ItemArray.Length; i++)
                    {
                        if (dataRow.ItemArray[i] != DBNull.Value) total += (int)dataRow.ItemArray[i];
                    }
                    r["Score"] = total;
                }
            }
        }
    }
}
