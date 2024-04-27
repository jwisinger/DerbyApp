#warning FUN: Need to add the 3 lane options too

using System.Collections.ObjectModel;

namespace DerbyApp.RaceStats
{
    public class RaceHeats
    {
        public static readonly int DefaultHeat = 0;
        public static readonly ObservableCollection<RaceHeat> Heats = new()
        {
            new RaceHeat("Thirteen Cars Four Lanes", 13, 13, 4, new int[][]
            {
                new int[] { 0, 12, 9, 7 },
                new int[] { 1, 0, 10, 8 },
                new int[] { 2, 1, 11, 9 },
                new int[] { 3, 2, 12, 10 },
                new int[] { 4, 3, 0, 11 },
                new int[] { 5, 4, 1, 12 },
                new int[] { 6, 5, 2, 0 },
                new int[] { 7, 6, 3, 1 },
                new int[] { 8, 7, 4, 2 },
                new int[] { 9, 8, 5, 3 },
                new int[] { 10, 9, 6, 4 },
                new int[] { 11, 10, 7, 5 },
                new int[] { 12, 11, 8, 6 },
            }),
            new RaceHeat("Seven Cars Four Lanes", 7, 7, 4, new int[][]
            {
                new int[] { 0, 6, 5, 2 },
                new int[] { 1, 0, 6, 3 },
                new int[] { 2, 1, 0, 4 },
                new int[] { 3, 2, 1, 5 },
                new int[] { 4, 3, 2, 6 },
                new int[] { 5, 4, 3, 0 },
                new int[] { 6, 5, 4, 1 },
            }),
            new RaceHeat("Five Cars Four Lanes", 5, 5, 4, new int[][]
            {
                new int[] { 0, 4, 2, 1 },
                new int[] { 1, 0, 3, 2 },
                new int[] { 2, 1, 4, 3},
                new int[] { 3, 2, 0, 4 },
                new int[] { 4, 3, 1, 0 },
            }),
            new RaceHeat("Four Cars Four Lanes", 4, 4, 4, new int[][]
            {
                new int[] { 0, 3, 2, 1 },
                new int[] { 1, 0, 3, 2 },
                new int[] { 2, 1, 0, 3 },
                new int[] { 3, 2, 1, 0 },
            })
        };
    }
}
