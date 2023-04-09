using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerbyApp
{
    public class Race
    {
        public string RaceName = "";
        public List<Racer> Racers = new();
        public bool InProgress = false;

        public Race() { }

        public Race(string raceName, List<Racer> racers, bool inProgress)
        {
            RaceName = raceName;
            Racers = racers;
            InProgress = inProgress;
        }
    }
}
