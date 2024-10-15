namespace DerbyApp.RaceStats
{
    internal class GirlScoutLevel(string level)
    {
        public string Level { get; set; } = level;
        public bool IsSelected { get; set; } = true;
    }
}
