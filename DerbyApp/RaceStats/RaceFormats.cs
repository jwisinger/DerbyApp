using System.Collections.ObjectModel;

namespace DerbyApp.RaceStats
{
    public class RaceFormats
    {
        public static readonly int DefaultFormat = 0;
        private static readonly ObservableCollection<RaceFormat> formats =
        [
            new RaceFormat("Thirteen Cars Four Lanes", 13, 13, 4,
            [
                [0, 12, 9, 7],
                [1, 0, 10, 8],
                [2, 1, 11, 9],
                [3, 2, 12, 10],
                [4, 3, 0, 11],
                [5, 4, 1, 12],
                [6, 5, 2, 0],
                [7, 6, 3, 1],
                [8, 7, 4, 2],
                [9, 8, 5, 3],
                [10, 9, 6, 4],
                [11, 10, 7, 5],
                [12, 11, 8, 6],
            ]),
            new RaceFormat("Seven Cars Four Lanes", 7, 7, 4,
            [
                [0, 6, 5, 2],
                [1, 0, 6, 3],
                [2, 1, 0, 4],
                [3, 2, 1, 5],
                [4, 3, 2, 6],
                [5, 4, 3, 0],
                [6, 5, 4, 1],
            ]),
            new RaceFormat("Five Cars Four Lanes", 5, 5, 4,
            [
                [0, 4, 2, 1],
                [1, 0, 3, 2],
                [2, 1, 4, 3],
                [3, 2, 0, 4],
                [4, 3, 1, 0],
            ]),
            new RaceFormat("Four Cars Four Lanes", 4, 4, 4,
            [
                [0, 3, 2, 1],
                [1, 0, 3, 2],
                [2, 1, 0, 3],
                [3, 2, 1, 0],
            ]),
            new RaceFormat("Seven Cars Three Lanes", 7, 7, 3,
            [
                [0, 6, 4],
                [1, 0, 5],
                [2, 1, 6],
                [3, 2, 1],
                [4, 3, 2],
                [5, 4, 3],
                [6, 5, 4],
            ]),
            new RaceFormat("Four Cars Three Lanes", 4, 4, 3,
            [
                [0, 3, 2],
                [1, 0, 3],
                [2, 1, 0],
                [3, 2, 1],
            ]),
            new RaceFormat("Three Cars Three Lanes", 3, 3, 3,
            [
                [0, 2, 1],
                [1, 0, 2],
                [2, 1, 0],
            ])
        ];

        public static ObservableCollection<RaceFormat> Formats => formats;
    }
}
