using System.Collections.ObjectModel;

namespace DerbyApp.RaceStats
{
    internal class GirlScoutLevels
    {
        public static ObservableCollection<GirlScoutLevel> ScoutLevels { get; set; } =
            [
                new("Daisy"),
                new("Brownie"),
                new("Junior"),
                new("Cadette"),
                new("Senior"),
                new("Ambassador"),
                new("Adult")
            ];
    }
}
