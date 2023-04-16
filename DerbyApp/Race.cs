using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;

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
            Ldrboard = new LeaderBoard();
            CurrentHeat = new Heat();
            RaceResultsTable = new DataTable();
            RaceResultsTable.Columns.Add("Number");
            RaceResultsTable.Columns.Add("Name");
            for(int i = 1; i <= heatlist.RacerCount; i++)
            {
                RaceResultsTable.Columns.Add("Heat " + i);
            }
            foreach (Racer r in racers)
            {
                DataRow row = RaceResultsTable.NewRow();
                row["Number"] = r.Number;
                row["Name"] = r.RacerName;
                /*for (int i = 0; i < heatlist.RacerCount; i++)
                {
                    int val = Array.IndexOf(heatlist.Heats[i], racerNum);
                    if (val >= 0) row["Heat " + (i + 1)] = val + 1;
                    else row["Heat " + (i + 1)] = " ";
                }*/
                RaceResultsTable.Rows.Add(row);
                racerNum++;
            }
        }
    }
}
