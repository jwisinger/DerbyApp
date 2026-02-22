namespace DerbyApp.RaceStats
{
    public class GirlScoutLevel(string level)
    {
        public string Level { get; set; } = level;
        public bool IsSelected { get; set; } = true;
    }
}
