using DerbyApp.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DerbyApp.RaceStats
{
    public class Leaderboard(TrulyObservableCollection<Racer> racers, int laneCount, bool timeBasedScoring)
    {
        public TrulyObservableCollection<Racer> Board = racers;// [.. racers];
        public bool TimeBasedScoring = timeBasedScoring;
        private readonly int _laneCount = laneCount;

#warning FUTURE: Use or delete this
        public List<Racer> CheckForTie(int positionToCheck)
        {
            return [.. Board.Where(x => x.Score == Board[positionToCheck].Score)];
        }

        public void CalculateResults(DataTable raceResultsTable)
        {
            if (TimeBasedScoring) CalculateResultsTimeBased(raceResultsTable);
            else CalculateResultsPlaceBased(raceResultsTable);
        }

        private void CalculateResultsPlaceBased(DataTable raceResultsTable)
        {
            DataTable raceScoreTable = raceResultsTable.Copy();

            foreach (DataColumn dc in raceResultsTable.Columns)
            {
                if (dc.ColumnName.Contains("Heat"))
                {
                    List<Tuple<float, int>> l = [];
                    foreach (DataRow dataRow in raceResultsTable.Rows)
                    {
                        if (dataRow[dc] == DBNull.Value)
                        {
                            continue;
                        }
                        try
                        {
                            l.Add(Tuple.Create((float)dataRow[dc], (int)dataRow["Number"]));
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError("Leaderboard.CalculateResultsPlaceBased", ex);
                        }
                    }
                    l.Sort();

                    foreach (DataRow dataRow in raceScoreTable.Rows)
                    {
                        int index = l.FindIndex(x => x.Item2 == (int)dataRow["Number"]);
                        if (index >= 0) dataRow[dc.Ordinal] = _laneCount - index;
                    }
                }
            }

            foreach (DataRow dataRow in raceScoreTable.Rows)
            {
                Racer r = Board.Where(x => x.Number == (int)dataRow["Number"]).FirstOrDefault();
                if (r != null)
                {
                    float total = 0;
                    for (int i = 2; i < dataRow.ItemArray.Length; i++)
                    {
                        if (dataRow.ItemArray[i] != DBNull.Value) total += (float)dataRow.ItemArray[i];
                    }
                    r.Score = (int)total;
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
                    if (obj.GetType() == typeof(float)) total += (float)obj;
                }

                Racer r = Board.Where(x => x.Number == (int)dataRow["Number"]).FirstOrDefault();
                if (r != null) r.Score = (decimal)total;
            }
        }
    }
}
