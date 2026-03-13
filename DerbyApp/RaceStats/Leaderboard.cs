using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DerbyApp.Helpers;

namespace DerbyApp.RaceStats
{
    public class Leaderboard(TrulyObservableCollection<Racer> racers, int laneCount, bool timeBasedScoring)
    {
        public struct HeatResult : IComparable<HeatResult>
        {
            public int RacerNumber;
            public int HeatNumber;
            public int Position;
            public float Time;

            public readonly int CompareTo(HeatResult other)
            {
                return Time.CompareTo(other.Time);
            }
        }

        public struct RaceResults
        {
            public string RaceName;
            public int OverallPosition;
            public bool Tie;
            public List<HeatResult> HeatResults;
        }

        public struct RacerResults
        {
            public Racer Racer;
            public List<RaceResults> Results;
        }

        public TrulyObservableCollection<Racer> Board = racers;
        public bool TimeBasedScoring = timeBasedScoring;
        private readonly int _laneCount = laneCount;

        public List<Racer> CheckForTie(int positionToCheck)
        {
            return [.. Board.Where(x => x.Score == Board[positionToCheck].Score)];
        }

        public RaceResults GetRacerResults(Racer r, DataTable raceResultsTable)
        {
            int heatNumber = 0;
            RaceResults raceResults = new()
            {
                HeatResults = [],
                OverallPosition = Board.IndexOf(Board.Where(x => x.Number == r.Number).FirstOrDefault()),
                Tie = false
            };

            if (raceResults.OverallPosition > -1)
            {
                if (CheckForTie(raceResults.OverallPosition).Count > 1) raceResults.Tie = true;
                foreach (DataColumn dc in raceResultsTable.Columns)
                {
                    if (dc.ColumnName.Contains("Heat"))
                    {
                        heatNumber++;
                        List<HeatResult> heatRacerList = [];
                        foreach (DataRow dataRow in raceResultsTable.Rows)
                        {
                            if (dataRow[dc] != DBNull.Value)
                            {
                                try
                                {
                                    heatRacerList.Add(new HeatResult() { HeatNumber = heatNumber, Time = (float)dataRow[dc], RacerNumber = (int)dataRow["Number"] });
                                }
                                catch (Exception ex)
                                {
                                    ErrorLogger.LogError("Leaderboard.GetRacerResults", ex);
                                }
                            }
                        }
                        heatRacerList.Sort();

                        int i = heatRacerList.FindIndex(x => x.RacerNumber == r.Number);
                        if (i > -1)
                        {
                            HeatResult heatResult = heatRacerList[i];
                            heatResult.Position = i + 1;
                            raceResults.HeatResults.Add(heatResult);
                        }
                    }
                }
            }

            return raceResults;
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
                    List<Tuple<float, int>> heatRacerList = [];
                    foreach (DataRow dataRow in raceResultsTable.Rows)
                    {
                        if (dataRow[dc] != DBNull.Value)
                        {
                            try
                            {
                                heatRacerList.Add(Tuple.Create((float)dataRow[dc], (int)dataRow["Number"]));
                            }
                            catch (Exception ex)
                            {
                                ErrorLogger.LogError("Leaderboard.CalculateResultsPlaceBased", ex);
                            }
                        }
                    }
                    heatRacerList.Sort();

                    foreach (DataRow dataRow in raceScoreTable.Rows)
                    {
                        int index = heatRacerList.FindIndex(x => x.Item2 == (int)dataRow["Number"]);
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

                object o = dataRow["Number"];
                long number = 0;
                if (o is int i) number = i;
                else if (o is long l) number = l;
                Racer r = Board.Where(x => x.Number == number).FirstOrDefault();
                if (r != null) r.Score = (decimal)total;
            }
        }
    }
}
