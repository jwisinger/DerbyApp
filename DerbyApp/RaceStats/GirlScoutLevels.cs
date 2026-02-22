using System.Collections.ObjectModel;

namespace DerbyApp.RaceStats
{
    public class GirlScoutLevels
    {
        public ObservableCollection<GirlScoutLevel> ScoutLevels { get; set; } =
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
