using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace DerbyApp.RaceStats
{
    public class RaceResults : INotifyPropertyChanged
    {
        private int _currentHeatNumber = 1;

        public string RaceName;
        public ObservableCollection<Racer> Racers;
        public DataTable ResultsTable;
        public RaceFormat RaceFormat;
        public bool InProgress = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ColumnAdded;

        public int CurrentHeatNumber
        {
            get
            {
                return _currentHeatNumber;
            }
            set
            {
                _currentHeatNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentHeatNumber)));
            }
        }

        public RaceResults(string raceName, ObservableCollection<Racer> racers, RaceFormat raceFormat)
        {
            int racerNum = 0;

            RaceName = raceName;
            Racers = racers;
            RaceFormat = raceFormat;

            ResultsTable = new DataTable();
            ResultsTable.Columns.Add("Number", Type.GetType("System.Int32"));
            ResultsTable.PrimaryKey = [ResultsTable.Columns["Number"]];
            ResultsTable.Columns.Add("Name", Type.GetType("System.String"));
            for (int i = 1; i <= RaceFormat.HeatCount; i++)
            {
                ResultsTable.Columns.Add("Heat " + i, Type.GetType("System.Double"));
            }
            foreach (Racer r in racers)
            {
                DataRow row = ResultsTable.NewRow();
                row["Number"] = r.Number;
                row["Name"] = r.RacerName;
                ResultsTable.Rows.Add(row);
                r.RaceOrder = racerNum++;
            }
        }

        public void UpdateResults(string newString, int column, int row)
        {
            if (row >= ResultsTable.Rows.Count) return;
            if (!double.TryParse(newString, out _)) return;

            ResultsTable.Rows[row][column] = newString;
            InProgress = true;
        }

        public void AddRunOffHeat(List<Racer> racers)
        {
            RaceFormat.AddRunOffHeat(racers);
            ResultsTable.Columns.Add("Heat " + RaceFormat.HeatCount, Type.GetType("System.Double"));
            ColumnAdded?.Invoke(this, new PropertyChangedEventArgs("Heat " + RaceFormat.HeatCount));
        }
    }
}
