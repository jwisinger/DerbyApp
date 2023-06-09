﻿#warning TODO: Add abilty to run smaller races (at least 4 or less)

namespace DerbyApp.RaceStats
{
    internal class RaceHeats
    {
        public static readonly RaceHeat Default =
            new RaceHeat(13, 13, 4, new int[][]
            {
                        new int[] { 6, 10, 11, 2 },
                        new int[] { 11, 4, 12, 0 },
                        new int[] { 8, 0, 6, 9 },
                        new int[] { 4, 2, 7, 8 },
                        new int[] { 9, 11, 1, 7 },
                        new int[] { 1, 6, 5, 4 },
                        new int[] { 7, 12, 3, 6 },
                        new int[] { 2, 5, 9, 12 },
                        new int[] { 3, 9, 4, 10 },
                        new int[] { 10, 7, 0, 5 },
                        new int[] { 0, 1, 2, 3 },
                        new int[] { 12, 8, 10, 1 },
                        new int[] { 5, 3, 8, 11 },
            });
        public static readonly RaceHeat ThirteenCarsFourLanes =
            new RaceHeat(13, 13, 4, new int[][]
            {
                new int[] { 6, 10, 11, 2 },
                new int[] { 11, 4, 12, 0 },
                new int[] { 8, 0, 6, 9 },
                new int[] { 4, 2, 7, 8 },
                new int[] { 9, 11, 1, 7 },
                new int[] { 1, 6, 5, 4 },
                new int[] { 7, 12, 3, 6 },
                new int[] { 2, 5, 9, 12 },
                new int[] { 3, 9, 4, 10 },
                new int[] { 10, 7, 0, 5 },
                new int[] { 0, 1, 2, 3 },
                new int[] { 12, 8, 10, 1 },
                new int[] { 5, 3, 8, 11 },
            });
    }
}
