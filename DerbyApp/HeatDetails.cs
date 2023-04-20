using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace DerbyApp
{
    public class HeatDetails
    {
        private readonly HeatList _heatlist;
        private readonly List<Racer> _racers;

        public HeatDetails(HeatList heatlist, List<Racer> racers)
        {
            _heatlist = heatlist;
            _racers = racers;
        }

        public ObservableCollection<Racer> GetHeat(int number)
        {
            ObservableCollection<Racer> racers = new ObservableCollection<Racer>();
            int num = number - 1;

            if (num >= _heatlist.Heats.Length) return racers;

            for (int i = 0; i < _heatlist.Heats[num].Length; i++)
            {
                Racer racer = _racers.FirstOrDefault(r => r.RaceOrder == _heatlist.Heats[num][i]);
                if (racer != null) racers.Add(racer);
                else racers.Add(new Racer());

            }

            return racers;
        }
    }
}
