using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerbyApp
{
    public class HeatList
    {
        public readonly int RacerCount;
        public readonly int[][] Heats;

        public HeatList(int racerCount, int[][] heats)
        {
            RacerCount = racerCount;
            Heats = heats;
        }
    }
}
