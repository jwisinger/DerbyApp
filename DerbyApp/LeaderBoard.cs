using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DerbyApp
{
    public class LeaderBoard
    {
        public TrulyObservableCollection<Racer> Racers = new TrulyObservableCollection<Racer>();
        public LeaderBoard() : this(new List<Racer>()) { }

        public LeaderBoard(List<Racer> racers)
        {
            foreach (Racer r in racers)
            {
                Racers.Add(r);
            }
        }
    }
}
