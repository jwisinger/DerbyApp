namespace DerbyApp.RaceStats
{
    internal class GirlScoutLevel
    {
        public string Level { get; set; }
        public bool IsSelected { get; set; }
        public GirlScoutLevel(string level)
        {
            Level = level;
            IsSelected = true;
        }
    }
}
