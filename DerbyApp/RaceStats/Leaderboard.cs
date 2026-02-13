using DerbyApp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace DerbyApp.RaceStats
{
    public class Leaderboard
    {
        public DataTable RaceScoreTable;
        public TrulyObservableCollection<Racer> Board;
        public bool TimeBasedScoring = false;
        private readonly int _laneCount = 0;

        public Leaderboard(ObservableCollection<Racer> racers, int heatCount, int laneCount, bool timeBasedScoring)
        {
            int racerNum = 0;

            _laneCount = laneCount;
            Board = [.. racers];
            TimeBasedScoring = timeBasedScoring;

            RaceScoreTable = new DataTable();
            RaceScoreTable.Columns.Add("Number", Type.GetType("System.Int32"));
            RaceScoreTable.Columns.Add("Name", Type.GetType("System.String"));

            for (int i = 1; i <= heatCount; i++)
            {
                RaceScoreTable.Columns.Add("Heat " + i, Type.GetType("System.Int32"));
            }

            foreach (Racer r in racers)
            {
                DataRow row = RaceScoreTable.NewRow();
                row["Number"] = r.Number;
                row["Name"] = r.RacerName;
                RaceScoreTable.Rows.Add(row);
                r.RaceOrder = racerNum++;
            }
        }

        public List<Racer> CheckForTie(int positionToCheck)
        {
            return [.. Board.Where(x => x.Score == Board[positionToCheck].Score)];
        }

        public void AddRunOffHeat(int heatCount)
        {
            RaceScoreTable.Columns.Add("Heat " + heatCount, Type.GetType("System.Int32"));
        }

        public void CalculateResults(DataTable raceResultsTable, int heatCount)
        {
            for (int i = 1; i <= heatCount - 2; i++)
            {
                List<Tuple<double, string>> l = [];
                foreach (DataRow dataRow in raceResultsTable.Rows)
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

                foreach (DataRow dataRow in RaceScoreTable.Rows)
                {
                    int index = l.FindIndex(x => x.Item2 == (string)dataRow["Name"]);
                    if (index >= 0)
                    {
                        dataRow["Heat " + i] = _laneCount - index;
                    }
                }
            }

            if (TimeBasedScoring) CalculateResultsTimeBased(raceResultsTable);
            else CalculateResultsPlaceBased();
        }

        private void CalculateResultsPlaceBased()
        {
            foreach (DataRow dataRow in RaceScoreTable.Rows)
            {
                Racer r = Board.Where(x => x.Number == (int)dataRow["Number"]).FirstOrDefault();
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

        private void CalculateResultsTimeBased(DataTable raceResultsTable)
        {
            foreach (DataRow dataRow in raceResultsTable.Rows)
            {
                double total = 0;
                foreach (object obj in dataRow.ItemArray)
                {
                    if (obj.GetType() == typeof(double)) total += (double)obj;
                }

                Racer r = Board.Where(x => x.Number == (int)dataRow["Number"]).FirstOrDefault();
                if (r != null) r.Score = (decimal)total;
            }
        }
    }
}
