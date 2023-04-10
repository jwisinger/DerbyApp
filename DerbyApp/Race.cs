using System.Collections.Generic;
using System.Data;

#warning FEATURE: Add race schedule into the table (include N/A) if racer not involved in heat
#warning FEATURE: Add ability to run heats and update leader board (and totals) ... store into database each run
#warning FEATURE: Allow override of auto-recorded times just in case
#warning FEATURE: Auto pull times from the M5Stack

namespace DerbyApp
{
    public class Race
    {
        public string RaceName;
        public List<Racer> Racers;
        public bool InProgress;
        public DataTable RaceScheduleTable;

        public Race():this("", new List<Racer>(), RaceSchedule.ThirteenCarsFourLanes) { }

        public Race(string raceName, List<Racer> racers, HeatList heatlist)
        {
            RaceName = raceName;
            Racers = racers; 
            InProgress = false;
            RaceScheduleTable = new DataTable();
            RaceScheduleTable.Columns.Add("Number");
            RaceScheduleTable.Columns.Add("Name");
            for(int i = 1; i <= heatlist.RacerCount; i++)
            {
                RaceScheduleTable.Columns.Add("Heat " + i);
            }
            foreach (Racer r in racers)
            {
                RaceScheduleTable.Rows.Add(r.Number, r.RacerName);
            }
        }
    }
}
