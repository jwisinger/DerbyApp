using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DerbyApp.RaceStats
{
    public class RaceFormat(string name, int heatCount, int racerCount, int laneCount, int[][] heats)
    {
        public ObservableCollection<Racer> CurrentRacers = [];
        public string Name { get; } = name;
        public int HeatCount = heatCount;
        public readonly int RacerCount = racerCount;
        public readonly int LaneCount = laneCount;
        public int[][] Heats = heats;

        public void UpdateDisplayedHeat(int heatNumber, ObservableCollection<Racer> racers)
        {
            CurrentRacers.Clear();
            int num = heatNumber - 1;

            if (num >= HeatCount) return;
            if (num >= Heats.Length) return;

            for (int i = 0; i < Heats[num].Length; i++)
            {
                Racer racer = racers.FirstOrDefault(r => r.RaceOrder == Heats[num][i]);
                racer ??= new();
                racer.Lane = i switch
                {
                    0 => 'G',
                    1 => 'I',
                    2 => 'R',
                    3 => 'L',
                    _ => ' ',
                };
                CurrentRacers.Add(racer);
            }
        }

        public RaceFormat Clone()
        {
            int[][] heats = (int[][])Heats.Clone();
            return new RaceFormat(Name, HeatCount, RacerCount, LaneCount, heats);
        }

        public void AddRunOffHeat(List<Racer> racers)
        {
            HeatCount++;
            try
            {
                int[][] newHeats = new int[HeatCount][];
                for (int i = 0; i < HeatCount - 1; i++)
                {
                    newHeats[i] = new int[LaneCount];
                    for (int j = 0; j < LaneCount; j++)
                    {
                        newHeats[i][j] = Heats[i][j];
                    }
                }
                newHeats[HeatCount - 1] = new int[LaneCount];
                for (int i = 0; i < LaneCount; i++) newHeats[HeatCount - 1][i] = -1;
                int k = 0;
                if (racers != null)
                {
                    foreach (Racer r in racers)
                    {
                        newHeats[HeatCount - 1][k++] = (int)r.RaceOrder;
                        if (k > LaneCount) break;
                    }
                }
                Heats = newHeats;
            }
            catch { }
        }
    }
}
