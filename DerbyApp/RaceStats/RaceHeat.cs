using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DerbyApp.RaceStats
{
    public class RaceHeat
    {
        public ObservableCollection<Racer> CurrentRacers;

        public readonly int HeatCount;
        public readonly int RacerCount;
        public readonly int[][] Heats;

        public RaceHeat(int heatCount, int racerCount, int[][] heats)
        {
            HeatCount = heatCount;
            RacerCount = racerCount;
            Heats = heats;
            CurrentRacers = new ObservableCollection<Racer>();
        }

        public void UpdateHeat(int heatNumber, List<Racer> racers)
        {
            CurrentRacers.Clear();
            int num = heatNumber - 1;

            if (num >= HeatCount) return;

            for (int i = 0; i < Heats[num].Length; i++)
            {
                Racer racer = racers.FirstOrDefault(r => r.RaceOrder == Heats[num][i]);
                if (racer != null) CurrentRacers.Add(racer);
                else CurrentRacers.Add(new Racer());
            }
        }
    }
}
