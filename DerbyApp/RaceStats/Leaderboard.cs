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
        private readonly int _laneCount = 0;

        public Leaderboard(ObservableCollection<Racer> racers, int heatCount, int laneCount)
        {
            int racerNum = 0;

            _laneCount = laneCount;
            Board = new TrulyObservableCollection<Racer>();
            foreach (Racer r in racers) Board.Add(r);

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

        public void CalculateResults(DataTable raceResultsTable)
        {
            for (int i = 1; i <= raceResultsTable.Columns.Count - 2; i++)
            {
                List<Tuple<double, string>> l = new();
                foreach (DataRow dataRow in raceResultsTable.Rows)
                {
#warning TODO: Check before accessing array
                    if (dataRow["Heat " + i] == DBNull.Value)
                    {
                        continue;
                    }
                    try
                    {
#warning TODO: Check before accessing array
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

            foreach (DataRow dataRow in RaceScoreTable.Rows)
            {
                Racer r = Board.Where(x => x.Number == (int)dataRow["Number"]).FirstOrDefault();
                if (r != null)
                {
                    int total = 0;
                    for (int i = 2; i < dataRow.ItemArray.Length; i++)
                    {
#warning TODO: Check before accessing array
                        if (dataRow.ItemArray[i] != DBNull.Value) total += (int)dataRow.ItemArray[i];
                    }
                    r.Score = total;
                }
            }
        }
    }
}
